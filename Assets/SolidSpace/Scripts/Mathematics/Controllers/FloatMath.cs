using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SolidSpace.Mathematics
{
    public static class FloatMath
    {
        public const float TwoPI = (float) (2 * Math.PI);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float x, float y, float sin, float cos)
        {
            return new float2
            {
                x = x * cos - y * sin,
                y = x * sin + y * cos
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos(float angleRad, out float sin, out float cos)
        {
            sin = (float) Math.Sin(angleRad);
            cos = (float) Math.Cos(angleRad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMax(float a, float b, out float min, out float max)
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
        public static void MinMax(float a, float b, float c, float d, out float min, out float max)
        {
            MinMax(a, b, out var min0, out var max0);
            MinMax(c, d, out var min1, out var max1);
            min = min0 < min1 ? min0 : min1;
            max = max0 > max1 ? max0 : max1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(float2 a, float2 b)
        {
            return math.distance(a, b);
        }
    }
}