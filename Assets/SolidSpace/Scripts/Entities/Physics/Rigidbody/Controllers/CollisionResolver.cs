using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    internal static class CollisionResolver
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShapesIntersect(CenterRotationSize shapeA, CenterRotationSize shapeB)
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
        private static CenterRotationSize WorldToLocalSpace(CenterRotationSize parent, CenterRotationSize child)
        {
            child.rotation -= parent.rotation;
            child.center -= parent.center;
            FloatMath.SinCos(-parent.rotation, out var sin, out var cos);
            child.center = FloatMath.Rotate(child.center, sin, cos);
            
            return child;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SeparateAxisExists(float2 bounds, CenterRotationSize shape)
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
        private static bool AllGreaterOrEqual(float t, float v0, float v1, float v2, float v3, float v4)
        {
            return (v0 >= t) && (v1 >= t) && (v2 >= t) && (v3 >= t) && (v4 >= t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllLessOrEqual(float t, float v0, float v1, float v2, float v3, float v4)
        {
            return (v0 <= t) && (v1 <= t) && (v2 <= t) && (v3 <= t) && (v4 <= t);
        }
    }
}