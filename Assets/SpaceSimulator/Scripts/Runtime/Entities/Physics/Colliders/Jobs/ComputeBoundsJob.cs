using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    [BurstCompile]
    public struct ComputeBoundsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> colliderChunks;
        [ReadOnly] public NativeArray<int> boundsWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<ColliderComponent> colliderHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<FloatBounds> outputBounds;
        
        public void Execute(int chunkIndex)
        {
            var chunk = colliderChunks[chunkIndex];
            var writeOffset = boundsWriteOffsets[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var colliders = chunk.GetNativeArray(colliderHandle);
            var entityCount = chunk.Count;

            for (var i = 0; i < entityCount; i++)
            {
                var radius = colliders[i].radius;
                var position = positions[i].value;
                FloatBounds boundsData;
                boundsData.xMin = position.x - radius;
                boundsData.xMax = position.x + radius;
                boundsData.yMin = position.y - radius;
                boundsData.yMax = position.y + radius;
                
                outputBounds[writeOffset] = boundsData;

                writeOffset++;
            }
        }
    }
}