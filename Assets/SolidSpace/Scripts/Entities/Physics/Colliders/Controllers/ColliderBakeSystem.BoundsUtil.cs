using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    partial class ColliderBakeSystem
    {
        private struct BoundsUtil
        {
            public float2 FindBoundsMaxSize(NativeArray<float2> sizes)
            {
                if (sizes.Length == 0)
                {
                    return default;
                }

                var size = sizes[0];
                var xMax = size.x;
                var yMax = size.y;
                var colliderCount = sizes.Length;

                for (var i = 1; i < colliderCount; i++)
                {
                    size = sizes[i];

                    if (size.x > xMax)
                    {
                        xMax = size.x;
                    }

                    if (size.y > yMax)
                    {
                        yMax = size.y;
                    }
                }

                return new float2(xMax, yMax);
            }
            
            public static float2 Static_FindBoundsMaxSize(NativeArray<float2> sizes)
            {
                if (sizes.Length == 0)
                {
                    return default;
                }

                var size = sizes[0];
                var xMax = size.x;
                var yMax = size.y;
                var colliderCount = sizes.Length;

                for (var i = 1; i < colliderCount; i++)
                {
                    size = sizes[i];

                    if (size.x > xMax)
                    {
                        xMax = size.x;
                    }

                    if (size.y > yMax)
                    {
                        yMax = size.y;
                    }
                }

                return new float2(xMax, yMax);
            }

            public FloatBounds JoinBounds(NativeArray<FloatBounds> colliders)
            {
                if (colliders.Length == 0)
                {
                    return default;
                }

                var entityBounds = colliders[0];
                var xMin = entityBounds.xMin;
                var xMax = entityBounds.xMax;
                var yMin = entityBounds.yMin;
                var yMax = entityBounds.yMax;
                var colliderCount = colliders.Length;
                for (var i = 1; i < colliderCount; i++)
                {
                    entityBounds = colliders[i];

                    if (entityBounds.xMin < xMin)
                    {
                        xMin = entityBounds.xMin;
                    }

                    if (entityBounds.yMin < yMin)
                    {
                        yMin = entityBounds.yMin;
                    }

                    if (entityBounds.xMax > xMax)
                    {
                        xMax = entityBounds.xMax;
                    }

                    if (entityBounds.yMax > yMax)
                    {
                        yMax = entityBounds.yMax;
                    }
                }

                return new FloatBounds
                {
                    xMin = xMin,
                    xMax = xMax,
                    yMin = yMin,
                    yMax = yMax
                };
            }
            
            public static FloatBounds Static_JoinBounds(NativeArray<FloatBounds> colliders)
            {
                if (colliders.Length == 0)
                {
                    return default;
                }

                var entityBounds = colliders[0];
                var xMin = entityBounds.xMin;
                var xMax = entityBounds.xMax;
                var yMin = entityBounds.yMin;
                var yMax = entityBounds.yMax;
                var colliderCount = colliders.Length;
                for (var i = 1; i < colliderCount; i++)
                {
                    entityBounds = colliders[i];

                    if (entityBounds.xMin < xMin)
                    {
                        xMin = entityBounds.xMin;
                    }

                    if (entityBounds.yMin < yMin)
                    {
                        yMin = entityBounds.yMin;
                    }

                    if (entityBounds.xMax > xMax)
                    {
                        xMax = entityBounds.xMax;
                    }

                    if (entityBounds.yMax > yMax)
                    {
                        yMax = entityBounds.yMax;
                    }
                }

                return new FloatBounds
                {
                    xMin = xMin,
                    xMax = xMax,
                    yMin = yMin,
                    yMax = yMax
                };
            }
        }
    }
}