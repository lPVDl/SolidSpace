using System.Runtime.CompilerServices;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Physics.Velocity;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics.Raycast
{
    [BurstCompile]
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> raycasterChunks;
        [ReadOnly] public NativeArray<int> resultWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<ColliderBounds> colliders;
        [ReadOnly] public float deltaTime;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> resultEntities;
        [WriteOnly] public NativeArray<int> resultCounts;
        public void Execute(int chunkIndex)
        {
            var chunk = raycasterChunks[chunkIndex];
            var rayCount = chunk.Count;
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var writeOffset = resultWriteOffsets[chunkIndex];
            var colliderCount = colliders.Length;
            var hitCount = 0;
            
            for (var i = 0; i < rayCount; i++)
            {
                var velocity = velocities[i].value;
                var pos0 = positions[i].value;
                var pos1 = pos0 + velocity * deltaTime;
                MinMax(pos0.x, pos1.x, out var xMin, out var xMax);
                MinMax(pos0.y, pos1.y, out var yMin, out var yMax);

                for (var j = 0; j < colliderCount; j++)
                {
                    var collider = colliders[j];
                    if (!BoundsOverlap(xMin, xMax, collider.xMin, collider.xMax))
                    {
                        continue;
                    }
                    if (!BoundsOverlap(yMin, yMax, collider.yMin, collider.yMax))
                    {
                        continue;
                    }

                    resultEntities[writeOffset + hitCount] = entities[i];
                    hitCount++;

                    break;
                }
            }

            resultCounts[chunkIndex] = hitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MinMax(float a, float b, out float min, out float max)
        {
            if (a > b)
            {
                min = b;
                max = a;
            }
            else
            {
                min = a;
                max = b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool BoundsOverlap(float min0, float max0, float min1, float max1)
        {
            return (max1 >= min0) && (max0 >= min1);
        }
    }
}