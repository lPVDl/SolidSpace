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
    }
}