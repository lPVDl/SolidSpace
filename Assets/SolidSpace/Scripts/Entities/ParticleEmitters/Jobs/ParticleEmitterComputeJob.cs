using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.ParticleEmitters
{
    [BurstCompile]
    internal struct ParticleEmitterComputeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RandomValueComponent> randomHandle;
        [ReadOnly] public ComponentTypeHandle<ParticleEmitterComponent> emitterHandle;
        [ReadOnly] public float inTime;

        public ComponentTypeHandle<RepeatTimerComponent> timerHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ParticleEmitterData> outParticles;
        [WriteOnly] public NativeArray<int> outParticleCounts;

        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var writeOffset = inWriteOffsets[chunkIndex];
            var entityCount = chunk.Count;
            var emitters = chunk.GetNativeArray(emitterHandle);
            var positions = chunk.GetNativeArray(positionHandle);
            var timers = chunk.GetNativeArray(timerHandle);
            var randoms = chunk.GetNativeArray(randomHandle);

            ParticleEmitterData emitterData;
            emitterData.despawnTime = inTime + 5;
            
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
                var angle = FloatMath.TwoPI * randoms[i].value;
                emitterData.position = positions[i].value;
                var velocity = emitter.particleVelocity;
                FloatMath.SinCos(angle, out var sin, out var cos);
                emitterData.velocity = new float2(velocity * cos, velocity * sin);

                outParticles[writeOffset + emitCount] = emitterData;
                emitCount++;
            }

            outParticleCounts[chunkIndex] = emitCount;
        }
    }
}