using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Velcast
{
    [BurstCompile]
    public struct KovacRaycastJob<T> : IJobParallelFor where T : struct, IRaycastBehaviour
    {
        public T behaviour;

        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public BakedCollidersData inColliders;
        [ReadOnly] public NativeArray<int> inWriteOffsets;

        [NativeDisableParallelForRestriction] public NativeArray<ushort> hitStack;
        [ReadOnly] public int hitStackSize;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> outCounts;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var rayCount = chunk.Count;
            var writeOffset = inWriteOffsets[chunkIndex];
            var stackOffset = chunkIndex * hitStackSize;
            var jobHitCount = 0;
            
            behaviour.ReadChunk(chunk);

            for (var rayIndex = 0; rayIndex < rayCount && jobHitCount < rayCount; rayIndex++)
            {
                var ray = behaviour.GetRay(rayIndex);

                FloatBounds rayBounds;
                FloatMath.MinMax(ray.pos0.x, ray.pos1.x, out rayBounds.xMin, out rayBounds.xMax);
                FloatMath.MinMax(ray.pos0.y, ray.pos1.y, out rayBounds.yMin, out rayBounds.yMax);
                
                var x0 = ((int) rayBounds.xMin >> inColliders.grid.power) - inColliders.grid.anchor.x;
                var x1 = ((int) rayBounds.xMax >> inColliders.grid.power) - inColliders.grid.anchor.x;
                var y0 = ((int) rayBounds.yMin >> inColliders.grid.power) - inColliders.grid.anchor.y;
                var y1 = ((int) rayBounds.yMax >> inColliders.grid.power) - inColliders.grid.anchor.y;
                
                if (x1 < 0 || x0 >= inColliders.grid.size.x)
                {
                    continue;
                }
                
                if (y1 < 0 || y0 >= inColliders.grid.size.y)
                {
                    continue;
                }
                
                if ((x0 == x1) && (y0 == y1))
                {
                    var cellData = inColliders.cells[y0 * inColliders.grid.size.x + x0];
                    if (cellData.count == 0)
                    {
                        continue;
                    }

                    for (var j = 0; (j < cellData.count) && (jobHitCount < rayCount); j++)
                    {
                        var colliderIndex = inColliders.indices[cellData.offset + j];
                        
                        if (!RaycastCollider(rayBounds, colliderIndex))
                        {
                            continue;
                        }

                        var hitInfo = new KovacRayHit
                        {
                            rayIndex = rayIndex,
                            colliderIndex = colliderIndex,
                            ray = ray,
                            writeOffset = writeOffset + jobHitCount
                        };
                        
                        if (behaviour.TryRegisterHit(hitInfo))
                        {
                            jobHitCount++;
                        }
                    }

                    continue;
                }

                var cellTotal = inColliders.grid.size.x * inColliders.grid.size.y;
                var rayHitCount = 0;
                var memoryLimitReached = false;
                for (var yOffset = y0; (yOffset <= y1) && (!memoryLimitReached); yOffset++)
                {
                    for (var xOffset = x0; (xOffset <= x1) && (!memoryLimitReached); xOffset++)
                    {
                        var cellIndex = yOffset * inColliders.grid.size.x + xOffset;
                        if (cellIndex < 0 || cellIndex >= cellTotal)
                        {
                            continue;
                        }

                        var cellData = inColliders.cells[cellIndex];
                        if (cellData.count == 0)
                        {
                            continue;
                        }

                        for (var j = 0; j < cellData.count; j++)
                        {
                            var colliderIndex = inColliders.indices[cellData.offset + j];
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

                            if (!RaycastCollider(rayBounds, colliderIndex))
                            {
                                continue;
                            }

                            var hitInfo = new KovacRayHit
                            {
                                rayIndex = rayIndex,
                                ray = ray,
                                colliderIndex = colliderIndex,
                                writeOffset = writeOffset + jobHitCount
                            };

                            if (!behaviour.TryRegisterHit(hitInfo))
                            {
                                continue;
                            }
                            
                            rayHitCount++; jobHitCount++;
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
            var colliderBounds = inColliders.bounds[colliderIndex];
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
            var colliderShape = inColliders.shapes[colliderIndex];
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
    }
}