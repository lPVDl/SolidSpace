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
            chunk.GetPower(out var indexPower, out var itemPower);

            return (chunk.offset << 2) + index.itemId * (1 << itemPower);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ComputeOffset(AtlasChunk2D chunk, AtlasIndex index)
        {
            chunk.GetPower(out var indexPower, out var itemPower);
            var x = (chunk.offset.x << 2) + (index.itemId & ((1 << indexPower) - 1)) * (1 << itemPower);
            var y = (chunk.offset.y << 2) + (index.itemId >> indexPower) * (1 << itemPower);

            return new int2(x, y);
        }

    }
}