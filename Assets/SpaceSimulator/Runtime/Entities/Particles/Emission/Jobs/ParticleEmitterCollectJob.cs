using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    [BurstCompile]
    public struct ParticleEmitterCollectJob : IJob
    {
        [ReadOnly] public NativeArray<ParticleEmissionData> particles;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int> offsets;
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int> counts;

        [WriteOnly] public NativeArray<ParticleEmissionData> result;
        [WriteOnly] public NativeArray<int> resultAmount;
        
        public void Execute()
        {
            var entityCount = 0;
            var offsetCount = offsets.Length;
            for (var i = 0; i < offsetCount; i++)
            {
                var localAmount = counts[i];
                if (localAmount == 0)
                {
                    continue;
                }
                
                var offset = offsets[i];
                for (var j = 0; j < localAmount; j++)
                {
                    result[entityCount] = particles[offset + j];
                    entityCount++;
                }
            }

            resultAmount[0] = entityCount;
        }
    }
}