using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    [BurstCompile]
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public ColliderWorld inColliderWorld;
        [ReadOnly] public NativeArray<ArchetypeChunk> raycasterChunks;
        [ReadOnly] public NativeArray<int> resultWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
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
            var hitCount = 0;
            var worldPower = inColliderWorld.worldGrid.power;
            var worldAnchor = inColliderWorld.worldGrid.anchor;
            var worldSize = inColliderWorld.worldGrid.size;
            var cellTotal = worldSize.x * worldSize.y;

            for (var i = 0; i < rayCount; i++)
            {
                var velocity = velocities[i].value;
                var pos0 = positions[i].value;
                var pos1 = pos0 + velocity * deltaTime;
                
                FloatBounds ray;
                MinMax(pos0.x, pos1.x, out ray.xMin, out ray.xMax);
                MinMax(pos0.y, pos1.y, out ray.yMin, out ray.yMax);

                var x0 = ((int) ray.xMin >> worldPower) - worldAnchor.x;
                var y0 = ((int) ray.yMin >> worldPower) - worldAnchor.y;
                var x1 = ((int) ray.xMax >> worldPower) - worldAnchor.x;
                var y1 = ((int) ray.yMin >> worldPower) - worldAnchor.y;

                if (x1 < 0 || x0 >= worldSize.x)
                {
                    continue;
                }

                if (y1 < 0 || y0 >= worldSize.y)
                {
                    continue;
                }
                
                if ((x0 == x1) && (y0 == y1))
                {
                    var index = y0 * worldSize.x + x0;
                    if (Raycast(inColliderWorld, index, ray))
                    {
                        resultEntities[writeOffset + hitCount++] = entities[i];
                    }
                    
                    continue;
                }

                var isHit = false;
                for (var yOffset = y0; (yOffset <= y1) && (!isHit); yOffset++)
                {
                    for (var xOffset = x0; xOffset <= x1; xOffset++)
                    {
                        var index = yOffset * worldSize.x + xOffset;
                        if (index < 0 || index >= cellTotal)
                        {
                            continue;
                        }
                        
                        if (!Raycast(inColliderWorld, index, ray))
                        {
                            continue;
                        }
                        
                        resultEntities[writeOffset + hitCount++] = entities[i];
                        isHit = true;
                        break;
                    }
                }
            }

            resultCounts[chunkIndex] = hitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Raycast(ColliderWorld world, int cellIndex, FloatBounds ray)
        {
            var cellData = world.worldCells[cellIndex];
            if (cellData.count == 0)
            {
                return false;
            }

            for (var i = 0; i < cellData.count; i++)
            {
                var colliderIndex = world.colliderStream[cellData.offset + i];
                var collider = world.colliders[colliderIndex];

                if (!BoundsOverlap(ray.xMin, ray.xMax, collider.xMin, collider.xMax))
                {
                    continue;
                }

                if (!BoundsOverlap(ray.yMin, ray.yMax, collider.yMin, collider.yMax))
                {
                    continue;
                }

                return true;
            }

            return false;
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