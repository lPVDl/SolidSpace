using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    [BurstCompile]
    public struct BlitShapeLinear24Job : IJob
    {
        [ReadOnly] public NativeSlice<byte> inSourceSeedMask;
        [ReadOnly] public NativeSlice<ColorRGB24> inSourceTexture;
        [ReadOnly] public int2 inSourceTextureSize;
        [ReadOnly] public int2 inSourceSeedMaskOffset;
        [ReadOnly] public int2 inSourceSeedMaskSize;
        [ReadOnly] public int2 inSourceTextureOffset;
        [ReadOnly] public int2 inTargetSize;
        [ReadOnly] public int2 inTargetOffset;
        
        [ReadOnly] public byte inBlitShapeSeed;
        [ReadOnly] public int2 inBlitSize;
        
        [ReadOnly] public int inConnectionCount;
        [ReadOnly] public NativeSlice<byte2> inConnections;

        private Mask256 _shapeMask;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeSlice<ColorRGB24> outTargetTexture;
        
        public void Execute()
        {
            _shapeMask.SetBitTrue((byte) (inBlitShapeSeed - 1));
            for (var i = 0; i < inConnectionCount; i++)
            {
                var connection = inConnections[i];
                if (inBlitShapeSeed == connection.x)
                {
                    _shapeMask.SetBitTrue((byte) (connection.y - 1));
                }
            }
            
            var sourceSeedMaskOffset = inSourceSeedMaskOffset.y * inSourceSeedMaskSize.x + inSourceSeedMaskOffset.x;
            var sourceTextureOffset = inSourceTextureOffset.y * inSourceTextureSize.x + inSourceTextureOffset.x;
            var targetOffset = inTargetOffset.y * inTargetSize.x + inTargetOffset.x;
            
            for (var y = 0; y < inBlitSize.y; y++)
            {
                for (var x = 0; x < inBlitSize.x; x++)
                {
                    var maskColor = inSourceSeedMask[sourceSeedMaskOffset + x];
                    if (maskColor == 0 || !_shapeMask.HasBit((byte) (maskColor - 1)))
                    {
                        outTargetTexture[targetOffset + x] = default;
                        continue;
                    }

                    outTargetTexture[targetOffset + x] = inSourceTexture[sourceTextureOffset + x];
                }

                sourceSeedMaskOffset += inSourceSeedMaskSize.x;
                sourceTextureOffset += inSourceTextureSize.x;
                targetOffset += inTargetSize.x;
            }
        }
    }
}