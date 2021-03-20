using SpaceSimulator.Runtime.Entities.Physics;
using SpaceSimulator.Runtime.Entities.Randomization;
using SpaceSimulator.Runtime.Entities.RepeatTimer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    [BurstCompile]
    public struct ParticleEmitterComputeJob : IJobParallelFor
    {
        private const float TwoPI = (float)(2 * math.PI_DBL);
        
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public NativeArray<int> offsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RandomValueComponent> randomHandle;
        [ReadOnly] public float time;

        public ComponentTypeHandle<RepeatTimerComponent> timerHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ParticleEmissionData> resultParticles;
        [WriteOnly] public NativeArray<int> resultCounts;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var resultOffset = offsets[chunkIndex];
            var entityCount = chunk.Count;
            var positions = chunk.GetNativeArray(positionHandle);
            var timers = chunk.GetNativeArray(timerHandle);
            var randoms = chunk.GetNativeArray(randomHandle);

            ParticleEmissionData emissionData;
            emissionData.despawnTime = time + 5;
            
            var emitCount = 0;
            for (var i = 0; i < entityCount; i++)
            {
                var timer = timers[i];
                if (timer.counter == 0)
                {
                    continue;
                }

                timer.counter = 0;
                timers[i] = timer;
                
                var angle = TwoPI * randoms[i].value;
                emissionData.position = positions[i].value;
                emissionData.velocity = new float2(30 * math.cos(angle), 30 * math.sin(angle));

                resultParticles[resultOffset + emitCount] = emissionData;
                emitCount++;
            }

            resultCounts[chunkIndex] = emitCount;
        }
    }
}