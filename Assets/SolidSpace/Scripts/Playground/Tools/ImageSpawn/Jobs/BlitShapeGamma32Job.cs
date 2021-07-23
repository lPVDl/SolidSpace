using System;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Splitting;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ImageSpawn
{
    [BurstCompile]
    public struct BlitShapeGamma32Job : IJob
    {
        [ReadOnly] public NativeSlice<byte> inSourceSeedMask;
        [ReadOnly] public NativeSlice<Color32> inSourceTexture;
        [ReadOnly] public int2 inSourceSize;
        [ReadOnly] public int2 inSourceOffset;
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
            _shapeMask = SplittingUtil.BuildShapeMask(inBlitShapeSeed, inConnections, inConnectionCount);

            var sourceOffset = inSourceOffset.y * inSourceSize.x + inSourceOffset.x;
            var targetOffset = inTargetOffset.y * inTargetSize.x + inTargetOffset.x;
            
            for (var y = 0; y < inBlitSize.y; y++)
            {
                for (var x = 0; x < inBlitSize.x; x++)
                {
                    var maskColor = inSourceSeedMask[sourceOffset + x];
                    if (maskColor == 0 || !_shapeMask.HasBit((byte) (maskColor - 1)))
                    {
                        outTargetTexture[targetOffset + x] = default;
                        continue;
                    }
                    
                    var sourceColor = inSourceTexture[sourceOffset + x];
                    outTargetTexture[targetOffset + x] = new ColorRGB24
                    {
                        r = GammaToLinear(sourceColor.r),
                        g = GammaToLinear(sourceColor.g),
                        b = GammaToLinear(sourceColor.b)
                    };
                }

                sourceOffset += inSourceSize.x;
                targetOffset += inTargetSize.x;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GammaToLinear(byte color)
        {
            return (byte) (Math.Pow(color / 255f, 2.2f) * 255);
        }
    }
}