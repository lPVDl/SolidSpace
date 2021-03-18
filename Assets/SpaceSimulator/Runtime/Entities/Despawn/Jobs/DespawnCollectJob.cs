using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    [BurstCompile]
    public struct DespawnCollectJob : IJob
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int> offsets;
        [ReadOnly] public int inputCountsAmount;
        [ReadOnly] public NativeArray<int> inputCounts;
        [ReadOnly] public NativeArray<Entity> inputEntities;

        [WriteOnly] public NativeArray<Entity> outputEntities;
        [WriteOnly] public NativeArray<int> outputCount;
        
        public void Execute()
        {
            var resultCount = 0;

            for (var i = 0; i < inputCountsAmount; i++)
            {
                var localAmount = inputCounts[i];
                if (localAmount == 0)
                {
                    continue;
                }
                
                var globalOffset = offsets[i];
                for (var j = 0; j < localAmount; j++)
                {
                    outputEntities[resultCount] = inputEntities[globalOffset + j];
                    resultCount++;
                }
            }

            outputCount[0] = resultCount;
        }
    }
}