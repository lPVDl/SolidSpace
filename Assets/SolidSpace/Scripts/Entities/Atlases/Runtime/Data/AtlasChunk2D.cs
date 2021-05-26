using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    public struct AtlasChunk2D
    {
        public byte2 offset;
        
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