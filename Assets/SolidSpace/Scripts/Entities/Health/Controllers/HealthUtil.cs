using System;
using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public static class HealthUtil
    {
        // TODO: Remove NativeArray<byte> version, leave NativeSlice only.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasBit(NativeArray<byte> health, int healthOffset, int spriteWidth, int2 spritePoint)
        {
            var bytesPerLine = (int) Math.Ceiling(spriteWidth / 8f);
            var chunkIndex = healthOffset + bytesPerLine * spritePoint.y + spritePoint.x / 8;
            var chunkValue = health[chunkIndex];
            var pointBitMask = 1 << (spritePoint.x % 8);
            
            return (chunkValue & pointBitMask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasBit(NativeSlice<byte> health, int2 healthSize, int2 point)
        {
            var bytesPerLine = (int) Math.Ceiling(healthSize.x / 8f);
            var chunkIndex = bytesPerLine * point.y + point.x / 8;
            var chunkValue = health[chunkIndex];
            var pointMask = 1 << (point.x % 8);

            return (chunkValue & pointMask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearBit(NativeArray<byte> health, int healthOffset, int spriteWidth, int2 spritePoint)
        {
            var bytesPerLine = (int) Math.Ceiling(spriteWidth / 8f);
            var chunkIndex = healthOffset + bytesPerLine * spritePoint.y + spritePoint.x / 8;
            var chunkValue = health[chunkIndex];
            var pointBitMask = 1 << (spritePoint.x % 8);

            health[chunkIndex] = (byte) (chunkValue & ~pointBitMask);
        }
        
        public static void TextureToHealth(NativeArray<Color32> texture, int width, int height, NativeSlice<byte> output)
        {
            var bytesPerLine = (int) Math.Ceiling(width / 8f);
            var requiredByteCount = bytesPerLine * height;
            if (output.Length < requiredByteCount)
            {
                var message = $"{nameof(output)} must be at least {requiredByteCount}b to store frame {width}x{height}, but got {output.Length}b";
                throw new InvalidOperationException(message);
            }

            for (var y = 0; y < height; y++)
            {
                var textureOffset = width * y;
                var bitsOffset = bytesPerLine * y;
                
                for (var x = 0; x < width; x += 8)
                {
                    var bitChunk = 0;

                    for (var j = 0; j < 8 && (x + j < width); j++)
                    {
                        var color = texture[textureOffset + x + j];
                        if (color.r + color.g + color.b > 0)
                        {
                            bitChunk |= 1 << j;
                        }
                    }

                    output[bitsOffset + x / 8] = (byte) bitChunk;
                }
            }
        }

        public static void TextureToHealth(NativeArray<ColorRGB24> texture, int width, int height, NativeSlice<byte> output)
        {
            var bytesPerLine = (int) Math.Ceiling(width / 8f);
            var requiredByteCount = bytesPerLine * height;
            if (output.Length < requiredByteCount)
            {
                var message = $"{nameof(output)} must be at least {requiredByteCount}b to store frame {width}x{height}, but got {output.Length}b";
                throw new InvalidOperationException(message);
            }

            for (var y = 0; y < height; y++)
            {
                var textureOffset = width * y;
                var bitsOffset = bytesPerLine * y;
                
                for (var x = 0; x < width; x += 8)
                {
                    var bitChunk = 0;

                    for (var j = 0; j < 8 && (x + j < width); j++)
                    {
                        var color = texture[textureOffset + x + j];
                        if (color.r + color.g + color.b > 0)
                        {
                            bitChunk |= 1 << j;
                        }
                    }

                    output[bitsOffset + x / 8] = (byte) bitChunk;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRequiredByteCount(int width, int height)
        {
            return (int) Math.Ceiling(width / 8f) * height;
        }
    }
}