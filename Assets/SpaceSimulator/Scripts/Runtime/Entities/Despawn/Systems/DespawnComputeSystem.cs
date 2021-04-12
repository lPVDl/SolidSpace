using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnComputeSystem : SystemBase
    {
        private const int IterationCycle = 8;

        public NativeArray<Entity> ResultBuffer => _entities;
        public int ResultCount => _entityCount[0];
        
        private NativeArray<int> _entityCount;
        private NativeArray<Entity> _entities;
        private EntityQuery _query;
        private int _lastOffset;
        private SystemBaseUtil _util;
        
        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(typeof(DespawnComponent));
            _lastOffset = -1;
            _entities = _util.CreatePersistentArray<Entity>(4096);
            _entityCount = _util.CreatePersistentArray<int>(1);
            _entityCount[0] = 0;
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("CalculateChunkCount");
            var chunkCount = _query.CalculateChunkCount();
            Profiler.EndSample();
            
            Profiler.BeginSample("Create Compute Buffer");
            _lastOffset = (_lastOffset + 1) % IterationCycle;
            var rawChunks = _query.CreateArchetypeChunkArray(Allocator.Temp);
            var computeChunkCount = Mathf.CeilToInt((chunkCount - _lastOffset) / (float) IterationCycle);
            var computeChunks = _util.CreateTempJobArray<ArchetypeChunk>(computeChunkCount);
            var computeOffsets = _util.CreateTempJobArray<int>(computeChunkCount);
            var countsBuffer = _util.CreateTempJobArray<int>(computeChunkCount);
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
            Profiler.EndSample();

            Profiler.BeginSample("Update Entities Buffer");
            if (_entities.Length < entityCount)
            {
                _entities.Dispose();
                _entities = _util.CreatePersistentArray<Entity>(entityCount * 2);
            }
            Profiler.EndSample();

            Profiler.BeginSample("Compute and collect");
            var computeJob = new DespawnComputeJob
            {
                inChunks = computeChunks,
                despawnHandle = GetComponentTypeHandle<DespawnComponent>(true),
                entityHandle = GetEntityTypeHandle(),
                outEntityCounts = countsBuffer, 
                inWriteOffsets = computeOffsets,
                outEntities = _entities,
                time = (float)Time.ElapsedTime
            };
            var computeJobHandle = computeJob.Schedule(computeChunkCount, 32, Dependency);

            var collectJob = new SingleBufferedDataCollectJob<Entity>
            {
                inCounts = countsBuffer,
                inOffsets = computeOffsets,
                inOutData = _entities,
                outCount = _entityCount
            };
            var collectJobHandle = collectJob.Schedule(computeJobHandle);
            collectJobHandle.Complete();
            Profiler.EndSample();

            countsBuffer.Dispose();
            rawChunks.Dispose();
            computeChunks.Dispose();
            computeOffsets.Dispose();
        }

        protected override void OnDestroy()
        {
            _entities.Dispose();
            _entityCount.Dispose();
        }
    }
}