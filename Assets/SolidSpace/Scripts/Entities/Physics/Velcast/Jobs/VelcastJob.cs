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
                    if (Raycast(inColliderWorld, index, ray, out var colliderIndex))
                    {
                        WriteResult(writeOffset + hitCount++, new RaycastHit
                        {
                            raycasterEntity = entities[i],
                            raycasterArchetypeIndex = rayArchetype,
                            colliderIndex = colliderIndex,
                            raycastOrigin = new FloatRay(pos0, pos1)
                        });
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
                        
                        if (!Raycast(inColliderWorld, index, ray, out var colliderIndex))
                        {
                            continue;
                        }
                        
                        WriteResult(writeOffset + hitCount++, new RaycastHit
                        {
                            raycasterEntity = entities[i],
                            raycasterArchetypeIndex = rayArchetype,
                            colliderIndex = colliderIndex,
                            raycastOrigin = new FloatRay(pos0, pos1)
                        });

                        isHit = true;
                        break;
                    }
                }
            }

            outCounts[chunkIndex] = hitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Raycast(ColliderWorld world, int cellIndex, FloatBounds ray, out ushort colliderIndex)
        {
            colliderIndex = 0;
            
            var cellData = world.worldCells[cellIndex];
            if (cellData.count == 0)
            {
                return false;
            }

            for (var i = 0; i < cellData.count; i++)
            {
                colliderIndex = world.colliderStream[cellData.offset + i];
                var colliderBounds = world.colliderBounds[colliderIndex];

                if (!FloatMath.BoundsOverlap(ray.xMin, ray.xMax, colliderBounds.xMin, colliderBounds.xMax))
                {
                    continue;
                }

                if (!FloatMath.BoundsOverlap(ray.yMin, ray.yMax, colliderBounds.yMin, colliderBounds.yMax))
                {
                    continue;
                }

                var center = FloatMath.GetBoundsCenter(colliderBounds);
                var p0 = new float2(ray.xMin, ray.yMin) - center;
                var p1 = new float2(ray.xMax, ray.yMax) - center;
                var colliderShape = world.colliderShapes[colliderIndex];
                FloatMath.SinCos(-colliderShape.rotation, out var sin, out var cos);
                p0 = FloatMath.Rotate(p0, sin, cos);
                p1 = FloatMath.Rotate(p1, sin, cos);
                FloatMath.MinMax(p0.x, p1.x, out var xMin, out var xMax);
                FloatMath.MinMax(p0.y, p1.y, out var yMin, out var yMax);
                var halfSize = new float2(colliderShape.size.x / 2f, colliderShape.size.y / 2f);

                if (!FloatMath.BoundsOverlap(xMin, xMax, -halfSize.x, +halfSize.x))
                {
                    continue;
                }

                if (!FloatMath.BoundsOverlap(yMin, yMax, -halfSize.y, +halfSize.y))
                {
                    continue;
                }

                return true;
            }

            return false;
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