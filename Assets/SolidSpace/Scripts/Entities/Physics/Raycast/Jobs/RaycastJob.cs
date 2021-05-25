using System.Runtime.CompilerServices;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Raycast
{
    [BurstCompile]
    internal struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public ColliderWorld inColliderWorld;
        [ReadOnly] public NativeArray<ArchetypeChunk> inRaycasterChunks;
        [ReadOnly] public NativeArray<int> inResultWriteOffsets;
        [ReadOnly] public float inDeltaTime;

        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> resultEntities;
        [WriteOnly] public NativeArray<int> resultCounts;
        public void Execute(int chunkIndex)
        {
            var chunk = inRaycasterChunks[chunkIndex];
            var rayCount = chunk.Count;
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var writeOffset = inResultWriteOffsets[chunkIndex];
            var hitCount = 0;
            var worldPower = inColliderWorld.worldGrid.power;
            var worldAnchor = inColliderWorld.worldGrid.anchor;
            var worldSize = inColliderWorld.worldGrid.size;
            var cellTotal = worldSize.x * worldSize.y;

            for (var i = 0; i < rayCount; i++)
            {
                var velocity = velocities[i].value;
                var pos0 = positions[i].value;
                var pos1 = pos0 + velocity * inDeltaTime;
                
                FloatBounds ray;
                FloatMath.MinMax(pos0.x, pos1.x, out ray.xMin, out ray.xMax);
                FloatMath.MinMax(pos0.y, pos1.y, out ray.yMin, out ray.yMax);

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
                var colliderBounds = world.colliderBounds[colliderIndex];

                if (!BoundsOverlap(ray.xMin, ray.xMax, colliderBounds.xMin, colliderBounds.xMax))
                {
                    continue;
                }

                if (!BoundsOverlap(ray.yMin, ray.yMax, colliderBounds.yMin, colliderBounds.yMax))
                {
                    continue;
                }

                var centerX = (colliderBounds.xMax + colliderBounds.xMin) / 2f;
                var centerY = (colliderBounds.yMax + colliderBounds.yMin) / 2f;
                var p0 = new float2(ray.xMin - centerX, ray.yMin - centerY);
                var p1 = new float2(ray.xMax - centerX, ray.yMax - centerY);
                var colliderShape = world.colliderShapes[colliderIndex];
                FloatMath.SinCos(-colliderShape.rotation * FloatMath.TwoPI, out var sin, out var cos);
                p0 = FloatMath.Rotate(p0.x, p0.y, sin, cos);
                p1 = FloatMath.Rotate(p1.x, p1.y, sin, cos);
                FloatMath.MinMax(p0.x, p1.x, out var xMin, out var xMax);
                FloatMath.MinMax(p0.y, p1.y, out var yMin, out var yMax);
                var halfSize = new float2(colliderShape.size.x / 2f, colliderShape.size.y / 2f);

                if (!BoundsOverlap(xMin, xMax, -halfSize.x, +halfSize.x))
                {
                    continue;
                }

                if (!BoundsOverlap(yMin, yMax, -halfSize.y, +halfSize.y))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool BoundsOverlap(float min0, float max0, float min1, float max1)
        {
            return (max1 >= min0) && (max0 >= min1);
        }
    }
}