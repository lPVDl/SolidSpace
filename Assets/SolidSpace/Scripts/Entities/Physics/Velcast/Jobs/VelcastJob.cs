using System.Runtime.CompilerServices;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Velcast
{
    [BurstCompile]
    internal struct VelcastJob : IJobParallelFor
    {
        private struct RaycastHit
        {
            public Entity raycasterEntity;
            public ushort colliderIndex;
            public byte raycasterArchetypeIndex;
            public FloatRay raycastOrigin;
        }
        
        [ReadOnly] public ColliderWorld inColliderWorld;
        [ReadOnly] public NativeArray<ArchetypeChunk> inRaycasterChunks;
        [ReadOnly] public NativeArray<int> inResultWriteOffsets;
        [ReadOnly] public NativeArray<byte> inRaycasterArchetypeIndices;
        [ReadOnly] public float inDeltaTime;

        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;

        [NativeDisableParallelForRestriction] public NativeArray<ushort> hitStack;
        [ReadOnly] public int hitStackSize;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> outRaycasterEntities;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ushort> outColliderIndices;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<byte> outRaycasterArchetypeIndices;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<FloatRay> outRaycastOrigins;
        [WriteOnly] public NativeArray<int> outCounts;
        public void Execute(int chunkIndex)
        {
            var chunk = inRaycasterChunks[chunkIndex];
            var rayCount = chunk.Count;
            var rayArchetype = inRaycasterArchetypeIndices[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var writeOffset = inResultWriteOffsets[chunkIndex];
            var worldPower = inColliderWorld.worldGrid.power;
            var worldAnchor = inColliderWorld.worldGrid.anchor;
            var worldSize = inColliderWorld.worldGrid.size;
            var cellTotal = worldSize.x * worldSize.y;
            var stackOffset = chunkIndex * hitStackSize;
            var jobHitCount = 0;

            for (var i = 0; (i < rayCount) && (jobHitCount < rayCount); i++)
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
                    var cellData = inColliderWorld.worldCells[y0 * worldSize.x + x0];
                    if (cellData.count == 0)
                    {
                        continue;
                    }

                    for (var j = 0; (j < cellData.count) && (jobHitCount < rayCount); j++)
                    {
                        var colliderIndex = inColliderWorld.colliderStream[cellData.offset + j];
                        
                        if (!RaycastCollider(ray, colliderIndex))
                        {
                            continue;
                        }
                        
                        WriteResult(writeOffset + jobHitCount++, new RaycastHit
                        {
                            raycasterEntity = entities[i],
                            raycasterArchetypeIndex = rayArchetype,
                            colliderIndex = colliderIndex,
                            raycastOrigin = new FloatRay(pos0, pos1)
                        });
                    }

                    continue;
                }

                var rayHitCount = 0;
                var memoryLimitReached = false;
                for (var yOffset = y0; (yOffset <= y1) && (!memoryLimitReached); yOffset++)
                {
                    for (var xOffset = x0; (xOffset <= x1) && (!memoryLimitReached); xOffset++)
                    {
                        var cellIndex = yOffset * worldSize.x + xOffset;
                        if (cellIndex < 0 || cellIndex >= cellTotal)
                        {
                            continue;
                        }

                        var cellData = inColliderWorld.worldCells[cellIndex];
                        if (cellData.count == 0)
                        {
                            continue;
                        }

                        for (var j = 0; j < cellData.count; j++)
                        {
                            var colliderIndex = inColliderWorld.colliderStream[cellData.offset + j];
                            var alreadyHit = false;
                            if (rayHitCount > 0)
                            {
                                for (var q = 0; q < rayHitCount; q++)
                                {
                                    if (hitStack[stackOffset + q] == colliderIndex)
                                    {
                                        alreadyHit = true;
                                        break;
                                    }
                                }

                                if (alreadyHit)
                                {
                                    continue;
                                }
                            }

                            if (!RaycastCollider(ray, colliderIndex))
                            {
                                continue;
                            }
                            
                            WriteResult(writeOffset + jobHitCount++, new RaycastHit
                            {
                                raycasterEntity = entities[i],
                                raycasterArchetypeIndex = rayArchetype,
                                colliderIndex = colliderIndex,
                                raycastOrigin = new FloatRay(pos0, pos1)
                            });
                            rayHitCount++;

                            if (jobHitCount >= rayCount || rayHitCount >= hitStackSize)
                            {
                                memoryLimitReached = true;
                                break;
                            }
                        }
                    }
                }
            }

            outCounts[chunkIndex] = jobHitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RaycastCollider(FloatBounds ray, ushort colliderIndex)
        {
            var colliderBounds = inColliderWorld.colliderBounds[colliderIndex];
            if (!FloatMath.BoundsOverlap(ray.xMin, ray.xMax, colliderBounds.xMin, colliderBounds.xMax))
            {
                return false;
            }

            if (!FloatMath.BoundsOverlap(ray.yMin, ray.yMax, colliderBounds.yMin, colliderBounds.yMax))
            {
                return false;
            }
            
            var center = FloatMath.GetBoundsCenter(colliderBounds);
            var p0 = new float2(ray.xMin, ray.yMin) - center;
            var p1 = new float2(ray.xMax, ray.yMax) - center;
            var colliderShape = inColliderWorld.colliderShapes[colliderIndex];
            FloatMath.SinCos(-colliderShape.rotation, out var sin, out var cos);
            p0 = FloatMath.Rotate(p0, sin, cos);
            p1 = FloatMath.Rotate(p1, sin, cos);
            FloatMath.MinMax(p0.x, p1.x, out var xMin, out var xMax);
            FloatMath.MinMax(p0.y, p1.y, out var yMin, out var yMax);
            var halfSize = new float2(colliderShape.size.x / 2f, colliderShape.size.y / 2f);

            if (!FloatMath.BoundsOverlap(xMin, xMax, -halfSize.x, +halfSize.x))
            {
                return false;
            }

            if (!FloatMath.BoundsOverlap(yMin, yMax, -halfSize.y, +halfSize.y))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteResult(int offset, RaycastHit hit)
        {
            outRaycasterEntities[offset] = hit.raycasterEntity;
            outColliderIndices[offset] = hit.colliderIndex;
            outRaycasterArchetypeIndices[offset] = hit.raycasterArchetypeIndex;
            outRaycastOrigins[offset] = hit.raycastOrigin;
        }
    }
}