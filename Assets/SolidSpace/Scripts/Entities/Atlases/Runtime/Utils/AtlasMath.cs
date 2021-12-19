using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    public static class AtlasMath
    {
        public const int Max1DAtlasSize = 1024 * 1024;
        public const int Max2DAtlasSize = 1024;
        public const int Min1DEntitySize = 16;
        public const int Min2DEntitySize = 4;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeOffset(AtlasChunk1D chunk, AtlasIndex index)
        {
            return (chunk.offset << 2) + index.ReadItemId() * (1 << chunk.itemPower);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ComputeOffset(AtlasChunk2D chunk, AtlasIndex index)
        {
            var itemId = index.ReadItemId();
            var x = (chunk.offset.x << 2) + (itemId &  3) * (1 << chunk.itemPower);
            var y = (chunk.offset.y << 2) + (itemId >> 2) * (1 << chunk.itemPower);

            return new int2(x, y);
        }

    }
}