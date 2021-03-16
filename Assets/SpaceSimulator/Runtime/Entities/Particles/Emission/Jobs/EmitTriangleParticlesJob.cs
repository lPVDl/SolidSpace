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
    public struct EmitTriangleParticlesJob : IJobParallelFor
    {
        private const float TwoPI = (float)(2 * math.PI_DBL);
        
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RandomValueComponent> randomHandle;

        public ComponentTypeHandle<RepeatTimerComponent> timerHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<EmitParticleData> results;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var resultOffset = chunkIndex * chunk.Capacity;
            var chunkPositions = chunk.GetNativeArray(positionHandle);
            var chunkTimers = chunk.GetNativeArray(timerHandle);
            var chunkRandoms = chunk.GetNativeArray(randomHandle);
            var result = new EmitParticleData();

            for (var i = 0; i < entityCount; i++)
            {
                var timer = chunkTimers[i];
                
                if (timer.counter == 0)
                {
                    result.emit = false;
                }
                else
                {
                    timer.counter = 0;
                    chunkTimers[i] = timer;
                    
                    var angle = TwoPI * chunkRandoms[i].value;
                    result.position = chunkPositions[i].value;
                    result.velocity = new float2(math.cos(angle), math.sin(angle));
                    result.emit = true;
                }

                results[resultOffset + i] = result;
            }
        }
    }
}