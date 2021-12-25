using Unity.Mathematics;

namespace SolidSpace.Mathematics
{
    public struct byte2
    {
        public static readonly byte2 zero = new byte2(0, 0);
        
        public byte x;
        public byte y;

        public byte2(int x, int y)
        {
            this.x = (byte) x;
            this.y = (byte) y;
        }
        
        public static float2 operator+ (float2 a, byte2 b)
        {
            return new float2(a.x + b.x, a.y + b.y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}