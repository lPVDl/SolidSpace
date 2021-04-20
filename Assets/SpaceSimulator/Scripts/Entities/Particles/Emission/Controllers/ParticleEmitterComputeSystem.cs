using SpaceSimulator.Entities.EntityWorld;
using SpaceSimulator.Entities.Extensions;
using SpaceSimulator.Entities.Randomization;
using SpaceSimulator.Entities.RepeatTimer;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace SpaceSimulator.Entities.Particles.Emission
{
    public class ParticleEmitterComputeSystem : IEntitySystem, IParticleEmitterComputeSystem
    {
        private const int BufferChunkSize = 128;

        public ESystemType SystemType => ESystemType.Compute;

        public NativeArray<ParticleEmissionData> Particles => _particles;
        public int ParticleCount => _particleCount[0];
        
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;
        private NativeArray<ParticleEmissionData> _particles;
        private NativeArray<int> _particleCount;
        private NativeArrayUtil _util;

        public ParticleEmitterComputeSystem(IEntityManager entityManager, IEntityWorldTime time)
        {
            _entityManager = entityManager;
            _time = time;
        }

        public void Initialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ParticleEmitterComponent),
                typeof(PositionComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent)
            });
            _particles = _util.CreatePersistentArray<ParticleEmissionData>(BufferChunkSize);
            _particleCount = _util.CreatePersistentArray<int>(1);
            _particleCount[0] = 0;
        }

        public void Update()
        {
            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("ComputeEntityOffsets");
            var chunkCount = chunks.Length;
            var offsets = _util.CreateTempJobArray<int>(chunkCount);
            var counts = _util.CreateTempJobArray<int>(chunkCount);
            var maxEntityCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = maxEntityCount;
                maxEntityCount += chunks[i].Count;
            }
            Profiler.EndSample();
            
            _util.MaintainPersistentArrayLength(ref _particles, maxEntityCount, BufferChunkSize);

            Profiler.BeginSample("Compute And Collect Job");
            var computeJob = new ParticleEmitterComputeJob
            {
                inChunks = chunks,
                timerHandle = _entityManager.GetComponentTypeHandle<RepeatTimerComponent>(false),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = _entityManager.GetComponentTypeHandle<RandomValueComponent>(true),
                emittterHandle = _entityManager.GetComponentTypeHandle<ParticleEmitterComponent>(true),
                inTime = (float) _time.ElapsedTime,
                outParticles = _particles, 
                inWriteOffsets = offsets,
                outParticleCounts = counts,
            };
            var computeHandle = computeJob.Schedule(chunks.Length, 32);

            var collectJob = new SingleBufferedDataCollectJob<ParticleEmissionData>
            {
                inCounts = counts,
                inOffsets = offsets,
                inOutData = _particles,
                outCount = _particleCount
            };
            var collectHandle = collectJob.Schedule(computeHandle);
            collectHandle.Complete();
            Profiler.EndSample();

            chunks.Dispose();
            offsets.Dispose();
            counts.Dispose();
        }

        public void FinalizeSystem()
        {
            _particles.Dispose();
            _particleCount.Dispose();
        }
    }
}