using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    [BurstCompile]
    public struct DespawnComputeJob : IJobParallelFor
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public NativeArray<int> offsets;
        
        [ReadOnly] public ComponentTypeHandle<DespawnComponent> despawnHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly] public float time;
        
        [WriteOnly] public NativeArray<int> resultCounts;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> resultEntities;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityOffset = offsets[chunkIndex];
            var entityCount = chunk.Count;
            var despawns = chunk.GetNativeArray(despawnHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var resultCount = 0;

            for (var i = 0; i < entityCount; i++)
            {
                if (time < despawns[i].despawnTime)
                {
                    continue;
                }

                resultEntities[entityOffset + resultCount] = entities[i];
                resultCount++;
            }

            resultCounts[chunkIndex] = resultCount;
        }
    }
}