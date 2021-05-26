using System.Runtime.CompilerServices;

namespace SolidSpace.Mathematics
{
    public static class BinaryMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int a)
        {
            return (a != 0) && ((a & (a - 1)) == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfFour(int a)
        {
            if (!IsPowerOfTwo(a))
            {
                return false;
            }

            return (a & 0x55555555) != 0;
        }
    }
}