using SpaceSimulator.Runtime.Entities.Common;
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
    public class ParticleEmitterComputeSystem : SystemBase
    {
        private const int BufferChunkSize = 128;

        public NativeArray<ParticleEmissionData> Particles => _particles;
        public int ParticleCount => _particleCount[0];
        
        private EntityQuery _query;
        private NativeArray<ParticleEmissionData> _particles;
        private NativeArray<int> _particleCount;
        private SystemBaseUtil _util;

        protected override void OnStartRunning()
        {
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
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

        protected override void OnUpdate()
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
                timerHandle = GetComponentTypeHandle<RepeatTimerComponent>(),
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = GetComponentTypeHandle<RandomValueComponent>(true),
                emittterHandle = GetComponentTypeHandle<ParticleEmitterComponent>(true),
                inTime = (float)Time.ElapsedTime,
                outParticles = _particles, 
                inWriteOffsets = offsets,
                outParticleCounts = counts,
            };
            var computeHandle = computeJob.Schedule(chunks.Length, 32, Dependency);

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

        protected override void OnDestroy()
        {
            _particles.Dispose();
            _particleCount.Dispose();
        }
    }
}