using SolidSpace.Entities.Components;
using SolidSpace.Entities.Utilities;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Despawn
{
    public class TimeDespawnComputeSystem : IInitializable, IUpdatable
    {
        private const int CycleLength = 8;
        
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityDestructionBuffer _destructionBuffer;

        private EntityQuery _query;
        private NativeArray<Entity> _entities;
        private int _cycleIndex;
        private ProfilingHandle _profiler;
        private JobMemoryAllocator _jobMemory;

        public TimeDespawnComputeSystem(IEntityManager entityManager, IEntityWorldTime time, IProfilingManager profilingManager,
            IEntityDestructionBuffer destructionBuffer)
        {
            _entityManager = entityManager;
            _time = time;
            _profilingManager = profilingManager;
            _destructionBuffer = destructionBuffer;
        }
        
        public void OnInitialize()
        {
            _jobMemory = new JobMemoryAllocator();
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(typeof(TimeDespawnComponent));
            _cycleIndex = 0;
            _entities = NativeMemory.CreatePersistentArray<Entity>(4096);
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query chunks");
            var chunks = EntityQueryForJobUtil.PartialQueryWithOffsets(_query, CycleLength, ref _cycleIndex);
            _profiler.EndSample("Query chunks");

            _profiler.BeginSample("Update Entity Buffer");
            if (_entities.Length < chunks.entityCount)
            {
                _entities.Dispose();
                _entities = NativeMemory.CreatePersistentArray<Entity>(chunks.entityCount * 2);
            }
            _profiler.EndSample("Update Entity Buffer");

            _profiler.BeginSample("Compute & Collect");
            var computeJob = new TimeDespawnComputeJob
            {
                inChunks = chunks.chunks,
                inWriteOffsets = chunks.chunkOffsets,
                despawnHandle = _entityManager.GetComponentTypeHandle<TimeDespawnComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                outEntityCounts = _jobMemory.CreateNativeArray<int>(chunks.chunkCount), 
                outEntities = _entities,
                time = (float) _time.ElapsedTime
            };
            var computeJobHandle = computeJob.Schedule(chunks.chunkCount, 32);

            var collectJob = new DataCollectJobWithOffsets<Entity>
            {
                inDataCount = chunks.chunkCount,
                inCounts = computeJob.outEntityCounts,
                inOffsets = chunks.chunkOffsets,
                inOutData = _entities,
                outCount = _jobMemory.CreateNativeReference<int>()
            };
            var collectJobHandle = collectJob.Schedule(computeJobHandle);
            collectJobHandle.Complete();
            _profiler.EndSample("Compute & Collect");

            _destructionBuffer.ScheduleDestroy(new NativeSlice<Entity>(_entities, 0, collectJob.outCount.Value));
            
            _profiler.BeginSample("Disposal");
            chunks.Dispose();
            _jobMemory.DisposeAllocations();
            _profiler.EndSample("Disposal");
        }

        public void OnFinalize()
        {
            _entities.Dispose();
        }
    }
}