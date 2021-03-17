using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    [BurstCompile]
    public struct DespawnCollectJob : IJob
    {
        [ReadOnly] public int inputCountsAmount;
        [ReadOnly] public NativeArray<int> inputCounts;
        [ReadOnly] public NativeArray<Entity> inputEntities;
        [ReadOnly] public int inputEntitiesChunkSize;
        
        [WriteOnly] public NativeArray<Entity> outputEntities;
        [WriteOnly] public NativeArray<int> outputCount;
        
        public void Execute()
        {
            var entityIndex = 0;
            var resultIndex = 0;
            for (var countIndex = 0; countIndex < inputCountsAmount; countIndex++, entityIndex += inputEntitiesChunkSize)
            {
                var chunkEntityCount = inputCounts[countIndex];
                if (chunkEntityCount == 0)
                {
                    continue;
                }

                for (var j = 0; j < chunkEntityCount; j++)
                {
                    outputEntities[resultIndex] = inputEntities[entityIndex + j];
                    resultIndex++;
                }
            }

            outputCount[0] = resultIndex;
        }
    }
}