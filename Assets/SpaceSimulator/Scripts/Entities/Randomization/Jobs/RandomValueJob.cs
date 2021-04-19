using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Entities.Randomization
{
    [BurstCompile]
    public struct RandomValueJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public NativeArray<float> randomValues;
        [ReadOnly] public int randomIndex;
        
        public ComponentTypeHandle<RandomValueComponent> randomHandle;
        
        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var randomCount = randomValues.Length;
            var randomOffset = chunkIndex * chunk.Capacity + randomIndex;
            var chunksRandoms = chunk.GetNativeArray(randomHandle);
            var randomCom = new RandomValueComponent();
            
            for (var i = 0; i < entityCount; i++)
            {
                randomCom.value = randomValues[(randomOffset + i) % randomCount];
                chunksRandoms[i] = randomCom;
            }
        }
    }
}