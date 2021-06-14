using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Eraser
{
    [BurstCompile]
    public struct FindNearestEntityJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public float2 inSearchPoint;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;

        [WriteOnly] public NativeArray<float2> outNearestPositions;
        [WriteOnly] public NativeArray<Entity> outNearestEntities;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var entityCount = chunk.Count;
            var minDistance = float.MaxValue;
            var minPosition = float2.zero;
            Entity minEntity = default;

            for (var i = 0; i < entityCount; i++)
            {
                var position = positions[i].value;
                var distance = FloatMath.Distance(inSearchPoint, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minEntity = entities[i];
                    minPosition = position;
                }
            }

            outNearestPositions[chunkIndex] = minPosition;
            outNearestEntities[chunkIndex] = minEntity;
        }
    }
}