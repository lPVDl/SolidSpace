using System;
using System.Runtime.CompilerServices;

namespace SolidSpace.Entities.Splitting
{
    public static class SplittingUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mask256 Bake4NeighbourPixelConnectionMask()
        {
            return new Mask256
            {
                v0 = BakeMaskPartial(0),
                v1 = BakeMaskPartial(64),
                v2 = BakeMaskPartial(128),
                v3 = BakeMaskPartial(192),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long BakeMaskPartial(byte offset)
        {
            var resultMask = 0L;

            for (var i = 0; i < 64; i++)
            {
                if (CheckAll4NeighbourPixelsAreConnected((byte) (offset + i)))
                {
                    resultMask |= 1L << i;
                }
            }

            return resultMask;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckAll4NeighbourPixelsAreConnected(byte frame)
        {
            const byte p0 = 1;
            const byte p1 = 2;
            const byte p2 = 4;
            const byte p3 = 8;
            const byte p4 = 16;
            const byte p5 = 32;
            const byte p6 = 64;
            const byte p7 = 128;
            
            var frame0 = (byte) (frame & p0);
            var frame1 = (byte) (frame & p1);
            var frame2 = (byte) (frame & p2);
            var frame3 = (byte) (frame & p3);
            var frame4 = (byte) (frame & p4);
            var frame5 = (byte) (frame & p5);
            var frame6 = (byte) (frame & p6);
            var frame7 = (byte) (frame & p7);
            var fill = Math.Max(Math.Max(frame1, frame3), Math.Max(frame4, frame6));
            var target = frame1 | frame3 | frame4 | frame6;
            
            if ((fill & target) == target)
            {
                return true;
            }

            for (var i = 0; i < 6; i++)
            {
                var fillBefore = fill;
                
                // 5 6 7
                // 3   4
                // 0 1 2
                Fill8Bit(frame0, p1, p3, ref fill, frame);
                Fill8Bit(frame1, p0, p2, ref fill, frame);
                Fill8Bit(frame2, p1, p4, ref fill, frame);
                Fill8Bit(frame3, p0, p5, ref fill, frame);
                Fill8Bit(frame4, p2, p7, ref fill, frame);
                Fill8Bit(frame5, p3, p6, ref fill, frame);
                Fill8Bit(frame6, p5, p7, ref fill, frame);
                Fill8Bit(frame7, p4, p6, ref fill, frame);

                if (fillBefore == fill)
                {
                    break;
                }
            }

            return (fill & target) == target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fill8Bit(byte origin, byte sibling0, byte sibling1, ref byte fill, byte frame)
        {
            Fill8Bit(origin, sibling0, ref fill, frame);
            Fill8Bit(origin, sibling1, ref fill, frame);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fill8Bit(byte origin, byte sibling, ref byte fill, byte frame)
        {
            if ((origin & fill) != 0 && (sibling & frame) != 0)
            {
                fill |= sibling;
            }
        }
    }
}