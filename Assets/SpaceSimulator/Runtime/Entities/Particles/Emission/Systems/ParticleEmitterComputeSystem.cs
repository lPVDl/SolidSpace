using SpaceSimulator.Runtime.Entities.Extensions;
using SpaceSimulator.Runtime.Entities.Physics;
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

        public NativeArray<ParticleEmissionData> Particles => _particleBufferB;
        public int ParticleCount => _resultCountBuffer[0];
        
        private EntityQuery _query;
        private NativeArray<ParticleEmissionData> _particleBufferA;
        private NativeArray<ParticleEmissionData> _particleBufferB;
        private NativeArray<int> _resultCountBuffer;

        protected override void OnStartRunning()
        {
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ParticleEmitterComponent),
                typeof(PositionComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent)
            });
            _particleBufferA = new NativeArray<ParticleEmissionData>(BufferChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _particleBufferB = new NativeArray<ParticleEmissionData>(BufferChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _resultCountBuffer = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _resultCountBuffer[0] = 0;
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            var offsets = EntitiesUtil.CreateEntityOffsetsForJob(chunks, out var approximateEntityCount);
            var counts = new NativeArray<int>(offsets.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            
            Profiler.BeginSample("UpdateResultBufferSize");
            var requiredBufferCapacity = Mathf.CeilToInt(approximateEntityCount / (float) BufferChunkSize) * BufferChunkSize;
            if (_particleBufferA.Length < requiredBufferCapacity)
            {
                _particleBufferA.Dispose();
                _particleBufferB.Dispose();
                _particleBufferA = new NativeArray<ParticleEmissionData>(requiredBufferCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                _particleBufferB = new NativeArray<ParticleEmissionData>(requiredBufferCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }
            Profiler.EndSample();

            Profiler.BeginSample("Compute Job");
            var computeJob = new ParticleEmitterComputeJob
            {
                chunks = chunks,
                timerHandle = GetComponentTypeHandle<RepeatTimerComponent>(),
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = GetComponentTypeHandle<RandomValueComponent>(true),
                time = (float)Time.ElapsedTime,
                resultParticles = _particleBufferA, 
                offsets = offsets,
                resultCounts = counts,
            };
            var computeHandle = computeJob.Schedule(chunks.Length, 32, Dependency);

            var collectJob = new ParticleEmitterCollectJob
            {
                offsets = offsets,
                counts = counts, 
                particles = _particleBufferA, 
                result = _particleBufferB,
                resultAmount = _resultCountBuffer
            };
            var collectHandle = collectJob.Schedule(computeHandle);
            collectHandle.Complete();
            
            Profiler.EndSample();
        }

        protected override void OnDestroy()
        {
            _particleBufferA.Dispose();
            _particleBufferB.Dispose();
            _resultCountBuffer.Dispose();
        }
    }
}