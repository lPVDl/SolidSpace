using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SolidSpace.Entities.Despawn
{
    public class DespawnComputeSystem : IEntitySystem, IDespawnComputeSystem
    {
        private const int IterationCycle = 8;

        public ESystemType SystemType => ESystemType.Compute;

        public NativeArray<Entity> ResultBuffer => _entities;
        public int ResultCount => _entityCount[0];
        
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private NativeArray<int> _entityCount;
        private NativeArray<Entity> _entities;
        private int _lastOffset;
        private NativeArrayUtil _arrayUtil;
        private ProfilingHandle _profiler;

        public DespawnComputeSystem(IEntityManager entityManager, IEntityWorldTime time, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _time = time;
            _profilingManager = profilingManager;
        }
        
        public void Initialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(typeof(DespawnComponent));
            _lastOffset = -1;
            _entities = _arrayUtil.CreatePersistentArray<Entity>(4096);
            _entityCount = _arrayUtil.CreatePersistentArray<int>(1);
            _entityCount[0] = 0;
        }

        public void Update()
        {
            _profiler.BeginSample("Compute Chunk Count");
            var chunkCount = _query.CalculateChunkCount();
            _profiler.EndSample("Compute Chunk Count");
            
            _profiler.BeginSample("Create Compute Buffer");
            _lastOffset = (_lastOffset + 1) % IterationCycle;
            var rawChunks = _query.CreateArchetypeChunkArray(Allocator.Temp);
            var computeChunkCount = Mathf.CeilToInt((chunkCount - _lastOffset) / (float) IterationCycle);
            var computeChunks = _arrayUtil.CreateTempJobArray<ArchetypeChunk>(computeChunkCount);
            var computeOffsets = _arrayUtil.CreateTempJobArray<int>(computeChunkCount);
            var countsBuffer = _arrayUtil.CreateTempJobArray<int>(computeChunkCount);
            var entityCount = 0;
            var chunkIndex = 0;

            for (var offset = _lastOffset; offset < chunkCount; offset += IterationCycle)
            {
                var rawChunk = rawChunks[offset];
                computeOffsets[chunkIndex] = entityCount;
                computeChunks[chunkIndex] = rawChunk;
                entityCount += rawChunk.Count;
                chunkIndex++;
            }
            _profiler.EndSample("Create Compute Buffer");

            _profiler.BeginSample("Update Entity Buffer");
            if (_entities.Length < entityCount)
            {
                _entities.Dispose();
                _entities = _arrayUtil.CreatePersistentArray<Entity>(entityCount * 2);
            }
            _profiler.EndSample("Update Entity Buffer");

            _profiler.BeginSample("Compute & Collect");
            var computeJob = new DespawnComputeJob
            {
                inChunks = computeChunks,
                despawnHandle = _entityManager.GetComponentTypeHandle<DespawnComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                outEntityCounts = countsBuffer, 
                inWriteOffsets = computeOffsets,
                outEntities = _entities,
                time = (float) _time.ElapsedTime
            };
            var computeJobHandle = computeJob.Schedule(computeChunkCount, 32);

            var collectJob = new SingleBufferedDataCollectJob<Entity>
            {
                inCounts = countsBuffer,
                inOffsets = computeOffsets,
                inOutData = _entities,
                outCount = _entityCount
            };
            var collectJobHandle = collectJob.Schedule(computeJobHandle);
            collectJobHandle.Complete();
            _profiler.EndSample("Compute & Collect");

            countsBuffer.Dispose();
            rawChunks.Dispose();
            computeChunks.Dispose();
            computeOffsets.Dispose();
        }

        public void FinalizeSystem()
        {
            _entities.Dispose();
            _entityCount.Dispose();
        }
    }
}