using SpaceSimulator.Runtime.Entities.Extensions;
using SpaceSimulator.Runtime.Entities.Randomization;
using SpaceSimulator.Runtime.Entities.RepeatTimer;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class ParticleEmitterComputeSystem : IEntitySystem
    {
        private const int BufferChunkSize = 128;

        public ESystemType SystemType => ESystemType.Compute;

        public NativeArray<ParticleEmissionData> Particles => _particles;
        public int ParticleCount => _particleCount[0];
        
        private readonly IEntityWorld _world;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;
        private NativeArray<ParticleEmissionData> _particles;
        private NativeArray<int> _particleCount;
        private EntitySystemUtil _util;

        public ParticleEmitterComputeSystem(IEntityWorld world, IEntityWorldTime time)
        {
            _world = world;
            _time = time;
        }

        public void Initialize()
        {
            _query = _world.EntityManager.CreateEntityQuery(new ComponentType[]
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
                timerHandle = _world.EntityManager.GetComponentTypeHandle<RepeatTimerComponent>(false),
                positionHandle = _world.EntityManager.GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = _world.EntityManager.GetComponentTypeHandle<RandomValueComponent>(true),
                emittterHandle = _world.EntityManager.GetComponentTypeHandle<ParticleEmitterComponent>(true),
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