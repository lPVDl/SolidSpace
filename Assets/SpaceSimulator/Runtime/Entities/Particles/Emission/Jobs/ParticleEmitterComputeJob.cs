using SpaceSimulator.Runtime.Entities.Common;
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
        
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RandomValueComponent> randomHandle;
        [ReadOnly] public ComponentTypeHandle<ParticleEmitterComponent> emittterHandle;
        [ReadOnly] public float inTime;

        public ComponentTypeHandle<RepeatTimerComponent> timerHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ParticleEmissionData> outParticles;
        [WriteOnly] public NativeArray<int> outParticleCounts;

        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var writeOffset = inWriteOffsets[chunkIndex];
            var entityCount = chunk.Count;
            var emitters = chunk.GetNativeArray(emittterHandle);
            var positions = chunk.GetNativeArray(positionHandle);
            var timers = chunk.GetNativeArray(timerHandle);
            var randoms = chunk.GetNativeArray(randomHandle);

            ParticleEmissionData emissionData;
            emissionData.despawnTime = inTime + 5;
            
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

                var emitter = emitters[i];
                var angle = TwoPI * randoms[i].value;
                emissionData.position = positions[i].value;
                var velocity = emitter.particleVelocity;
                emissionData.velocity = new float2(velocity * math.cos(angle), velocity * math.sin(angle));

                outParticles[writeOffset + emitCount] = emissionData;
                emitCount++;
            }

            outParticleCounts[chunkIndex] = emitCount;
        }
    }
}