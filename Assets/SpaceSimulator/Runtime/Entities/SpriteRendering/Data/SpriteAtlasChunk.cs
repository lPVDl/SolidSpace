using System.Runtime.CompilerServices;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public struct SpriteAtlasChunk
    {
        public byte offsetX;
        public byte offsetY;
        
        private byte _power;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetPower(out int indexPower, out int itemPower)
        {
            indexPower = _power >> 4;
            itemPower = _power & 15;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPower(int indexPower, int itemPower)
        {
            _power = (byte) ((indexPower << 4) + (itemPower & 15));
        }
    }
}