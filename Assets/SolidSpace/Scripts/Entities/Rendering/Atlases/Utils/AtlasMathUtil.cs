using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Atlases
{
    public struct AtlasMathUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 ComputeOffset(AtlasChunk chunk, AtlasIndex index)
        {
            chunk.GetPower(out var indexPower, out var itemPower);
            var x = chunk.offsetX << 2 + (index.itemId & ((1 << indexPower) - 1)) * (1 << itemPower);
            var y = chunk.offsetY << 2 + (index.itemId >> indexPower) * (1 << itemPower);

            return new int2(x, y);
        }
    }
}