using System;
using SolidSpace.JobUtilities;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Splitting
{
    public static class FrameBitUtil
    {
        public static NativeArray<byte> ConvertToBitArray(NativeArray<Color32> texture, int width, int height)
        {
            var bytesPerLine = (int) Math.Ceiling(width / 8f);
            var bits = NativeMemory.CreateTempJobArray<byte>(bytesPerLine * height);
            
            for (var y = 0; y < height; y++)
            {
                var textureOffset = width * y;
                var bitsOffset = bytesPerLine * y;
                
                for (var x = 0; x < width; x += 8)
                {
                    var value = 0;

                    for (var j = 0; j < 8 && (x + j < width); j++)
                    {
                        var color = texture[textureOffset + x + j];
                        var colorSum = Math.Min(1, color.r + color.g + color.b);
                        value |= colorSum << j;
                    }

                    bits[bitsOffset + x / 8] = (byte) value;
                }
            }

            return bits;
        }
    }
}