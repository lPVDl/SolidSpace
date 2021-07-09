using System.Runtime.CompilerServices;

namespace SolidSpace.Entities.Splitting
{
    public struct Mask256
    {
        public long v0;
        public long v1;
        public long v2;
        public long v3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasBit(byte index)
        {
            if (index < 128)
            {
                if (index < 64)
                {
                    return (v0 & (1L << index)) != 0;
                }

                return (v1 & (1L << (index - 64))) != 0;
            }

            if (index < 192)
            {
                return (v2 & (1L << (index - 128))) != 0;
            }
            
            return (v3 & (1L << (index - 192))) != 0;
        }
    }
}