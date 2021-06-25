using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParticleEmitters
{
    internal class ParticleEmitterComputeSystem : IInitializable, IUpdatable, IParticleEmitterComputeSystem
    {
        private const int BufferChunkSize = 128;

        public NativeArray<ParticleEmitterData> Particles => _particles;
        public int ParticleCount => _particleCount.Value;
        
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private NativeArray<ParticleEmitterData> _particles;
        private NativeReference<int> _particleCount;
        private ProfilingHandle _profiler;

        public ParticleEmitterComputeSystem(IEntityManager entityManager, IEntityWorldTime time, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _time = time;
            _profilingManager = profilingManager;
        }

        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ParticleEmitterComponent),
                typeof(PositionComponent),
                typeof(RandomComponent),
                typeof(RepeatTimerComponent)
            });
            _particles = NativeMemory.CreatePersistentArray<ParticleEmitterData>(BufferChunkSize);
            _particleCount = NativeMemory.CreatePersistentReference(0);
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query Chunks");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query Chunks");

            _profiler.BeginSample("Entity Offsets");
            var chunkCount = chunks.Length;
            var offsets = NativeMemory.CreateTempJobArray<int>(chunkCount);
            var counts = NativeMemory.CreateTempJobArray<int>(chunkCount);
            var maxEntityCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = maxEntityCount;
                maxEntityCount += chunks[i].Count;
            }
            _profiler.EndSample("Entity Offsets");
            
            NativeMemory.MaintainPersistentArrayLength(ref _particles, new ArrayMaintenanceData
            {
                requiredCapacity = maxEntityCount,
                itemPerAllocation = BufferChunkSize,
                copyOnResize = false
            });

            _profiler.BeginSample("Compute & Collect");
            var computeJob = new ParticleEmitterComputeJob
            {
                inChunks = chunks,
                timerHandle = _entityManager.GetComponentTypeHandle<RepeatTimerComponent>(false),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = _entityManager.GetComponentTypeHandle<RandomComponent>(true),
                emitterHandle = _entityManager.GetComponentTypeHandle<ParticleEmitterComponent>(true),
                inTime = (float) _time.ElapsedTime,
                outParticles = _particles, 
                inWriteOffsets = offsets,
                outParticleCounts = counts,
            };
            var computeHandle = computeJob.Schedule(chunks.Length, 32);

            var collectJob = new DataCollectJobWithOffsets<ParticleEmitterData>
            {
                inCounts = counts,
                inOffsets = offsets,
                inOutData = _particles,
                outCount = _particleCount
            };
            var collectHandle = collectJob.Schedule(computeHandle);
            collectHandle.Complete();
            _profiler.EndSample("Compute & Collect");

            chunks.Dispose();
            offsets.Dispose();
            counts.Dispose();
        }

        public void OnFinalize()
        {
            _particles.Dispose();
            _particleCount.Dispose();
        }
    }
}