using System.Runtime.CompilerServices;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    [BurstCompile]
    public struct RigidbodyCollisionJob : IJobParallelFor
    {
        private struct ColliderShape
        {
            public float2 center;
            public float rotation;
            public float2 size;

            public ColliderShape(float2 center, float rotation, float2 size)
            {
                this.center = center;
                this.rotation = rotation;
                this.size = size;
            }
        }
        
        [ReadOnly] public BakedColliders inColliders;
        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public float inMotionHalfSpeed;

        [ReadOnly] public ComponentTypeHandle<RigidbodyComponent> rigidbodyHandle;

        [NativeDisableParallelForRestriction] public NativeArray<ushort> hitStack;
        [ReadOnly] public int hitStackSize;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> outMotion;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var entityCount = chunk.Count;
            var rigidbodies = chunk.GetNativeArray(rigidbodyHandle);
            var hitStackOffset = chunkIndex * hitStackSize;
            var right = new float2(1, 0);
            var left = new float2(-1, 0);

            for (var entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                var thisIndex = rigidbodies[entityIndex].colliderIndex;
                var thisShape = inColliders.shapes[thisIndex];
                var thisBounds = inColliders.bounds[thisIndex];
                var thisCenter = FloatMath.GetBoundsCenter(thisBounds);
                
                ColliderUtil.WorldToGrid(thisBounds.xMin, thisBounds.yMin, inColliders.grid, out var x0, out var y0);
                ColliderUtil.WorldToGrid(thisBounds.xMax, thisBounds.yMax, inColliders.grid, out var x1, out var y1);

                var motion = float2.zero;
                
                if (x0 == x1 && y0 == y1)
                {
                    var cellData = inColliders.cells[y0 * inColliders.grid.size.x + x0];
                    if (cellData.count > 1)
                    {
                        for (var i = 0; i < cellData.count; i++)
                        {
                            var otherIndex = inColliders.indices[cellData.offset + i];
                            if (otherIndex == thisIndex)
                            {
                                continue;
                            }

                            var otherBounds = inColliders.bounds[otherIndex];
                            if (!FloatMath.BoundsOverlap(thisBounds, otherBounds))
                            {
                                continue;
                            }

                            var otherCenter = FloatMath.GetBoundsCenter(otherBounds);
                            var otherShape = inColliders.shapes[otherIndex];
                            var shapeA = new ColliderShape(thisCenter, thisShape.rotation, thisShape.size);
                            var shapeB = new ColliderShape(otherCenter, otherShape.rotation, otherShape.size);
                            if (!ShapesIntersect(shapeA, shapeB))
                            {
                                continue;
                            }

                            var direction = thisCenter - otherCenter;
                            var directionMag = FloatMath.Magnitude(direction);
                            if (directionMag > float.Epsilon)
                            {
                                motion += direction / directionMag * inMotionHalfSpeed;
                            }
                            else
                            {  
                                motion += (thisIndex > otherIndex ? right : left) * inMotionHalfSpeed;
                            }
                        }
                    }
                }
                else
                {
                    var hitCount = 0;

                    for (var yOffset = y0; (yOffset <= y1) && (hitCount < hitStackSize); yOffset++)
                    {
                        for (var xOffset = x0; (xOffset <= x1) && (hitCount < hitStackSize); xOffset++)
                        {
                            var cellData = inColliders.cells[yOffset * inColliders.grid.size.x + xOffset];
                            if (cellData.count < 2)
                            {
                                continue;
                            }

                            for (var i = 0; i < (cellData.count) && (hitCount < hitStackSize); i++)
                            {
                                var otherIndex = inColliders.indices[cellData.offset + i];
                                if (otherIndex == thisIndex)
                                {
                                    continue;
                                }

                                if (HitStackContainsCollider(hitStackOffset, hitCount, otherIndex))
                                {
                                    continue;
                                }

                                var otherBounds = inColliders.bounds[otherIndex];
                                if (!FloatMath.BoundsOverlap(thisBounds, otherBounds))
                                {
                                    continue;
                                }

                                var otherCenter = FloatMath.GetBoundsCenter(otherBounds);
                                var otherShape = inColliders.shapes[otherIndex];
                                var shapeA = new ColliderShape(thisCenter, thisShape.rotation, thisShape.size);
                                var shapeB = new ColliderShape(otherCenter, otherShape.rotation, otherShape.size);
                                if (!ShapesIntersect(shapeA, shapeB))
                                {
                                    continue;
                                }

                                hitStack[hitStackOffset + hitCount++] = otherIndex;
                                var direction = thisCenter - otherCenter;
                                var directionMag = FloatMath.Magnitude(direction);
                                if (directionMag > float.Epsilon)
                                {
                                    motion += direction / directionMag * inMotionHalfSpeed;
                                }
                                else
                                {
                                    motion += (thisIndex > otherIndex ? right : left) * inMotionHalfSpeed;
                                }
                            }
                        }
                    }
                }

                outMotion[thisIndex] = motion;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool ShapesIntersect(ColliderShape shapeA, ColliderShape shapeB)
        {
            var child = WorldToLocalSpace(shapeA, shapeB);
            if (SeparateAxisExists(shapeA.size, child))
            {
                return false;
            }

            child = WorldToLocalSpace(shapeB, shapeA);
            if (SeparateAxisExists(shapeB.size, child))
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ColliderShape WorldToLocalSpace(ColliderShape parent, ColliderShape child)
        {
            child.rotation -= parent.rotation;
            child.center -= parent.center;
            FloatMath.SinCos(-parent.rotation, out var sin, out var cos);
            child.center = FloatMath.Rotate(child.center, sin, cos);
            
            return child;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool SeparateAxisExists(float2 bounds, ColliderShape shape)
        {
            FloatMath.SinCos(shape.rotation, out var sin, out var cos);
            var halfSize = shape.size * 0.5f;
            var p0 = shape.center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
            var p1 = shape.center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
            var p2 = shape.center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
            var p3 = shape.center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
            var halfBounds = bounds * 0.5f;
            
            if (AllGreaterOrEqual(+halfBounds.x, shape.center.x, p0.x, p1.x, p2.x, p3.x))
            {
                return true;
            }

            if (AllLessOrEqual(-halfBounds.x, shape.center.x, p0.x, p1.x, p2.x, p3.x))
            {
                return true;
            }

            if (AllGreaterOrEqual(+halfBounds.y, shape.center.y, p0.y, p1.y, p2.y, p3.y))
            {
                return true;
            }

            if (AllLessOrEqual(-halfBounds.y, shape.center.y, p0.y, p1.y, p2.y, p3.y))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool AllGreaterOrEqual(float t, float v0, float v1, float v2, float v3, float v4)
        {
            return (v0 >= t) && (v1 >= t) && (v2 >= t) && (v3 >= t) && (v4 >= t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool AllLessOrEqual(float t, float v0, float v1, float v2, float v3, float v4)
        {
            return (v0 <= t) && (v1 <= t) && (v2 <= t) && (v3 <= t) && (v4 <= t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool HitStackContainsCollider(int stackOffset, int hitCount, ushort collider)
        {
            if (hitCount == 0)
            {
                return false;
            }
            
            for (var i = 0; i < hitCount; i++)
            {
                if (hitStack[stackOffset + i] == collider)
                {
                    return true;
                }
            }

            return false;
        }
    }
}