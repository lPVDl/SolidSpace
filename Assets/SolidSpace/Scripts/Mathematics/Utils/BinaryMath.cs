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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstBitIndex16(ushort value)
        {
            if (value == 0)
            {
                return -1;
            }

            // 0000 0000 1111 1111
            if ((value & 255) != 0)
            {
                return UnsafeGetFirstBitIndex8((byte) value);
            }
            
            // 1111 1111 0000 0000
            return 8 + UnsafeGetFirstBitIndex8((byte) (value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstBitIndex8(byte value)
        {
            if (value == 0)
            {
                return -1;
            }

            return UnsafeGetFirstBitIndex8(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeGetFirstBitIndex8(byte value)
        {
            // 0000 1111
            if ((value & 15) != 0)
            {
                return UnsafeGetFirstBitIndex4(value);
            }

            // 1111 0000
            return 4 + UnsafeGetFirstBitIndex4((byte) (value >> 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeGetFirstBitIndex4(byte value4Bit)
        {
            // 0011
            if ((value4Bit & 3) != 0)
            {
                if ((value4Bit & 1) != 0)
                {
                    return 0;
                }

                return 1;
            }
            
            // 1100
            if ((value4Bit & 4) != 0)
            {
                return 2;
            }

            return 3;
        }
    }
}