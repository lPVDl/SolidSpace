using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Colliders
{
    [BurstCompile]
    internal struct ComputeBoundsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> colliderChunks;
        [ReadOnly] public NativeArray<int> boundsWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<SizeComponent> sizeHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<FloatBounds> outputBounds;
        
        public void Execute(int chunkIndex)
        {
            var chunk = colliderChunks[chunkIndex];
            var writeOffset = boundsWriteOffsets[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var sizes = chunk.GetNativeArray(sizeHandle);
            var entityCount = chunk.Count;

            for (var i = 0; i < entityCount; i++)
            {
                var size = sizes[i].value;
                var position = positions[i].value;
                FloatBounds boundsData;
                boundsData.xMin = position.x - size.x / 2f;
                boundsData.xMax = position.x + size.x / 2f;
                boundsData.yMin = position.y - size.y / 2f;
                boundsData.yMax = position.y + size.y / 2f;
                
                outputBounds[writeOffset] = boundsData;

                writeOffset++;
            }
        }
    }
}