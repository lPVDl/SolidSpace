using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasConfigValidator : IValidator<SpriteAtlasConfig>
    {
        private readonly HashSet<int> _spriteSizeCash;

        public SpriteAtlasConfigValidator()
        {
            _spriteSizeCash = new HashSet<int>();
        }
        
        public void Validate(SpriteAtlasConfig data, ValidationResult result)
        {
            var chunks = data.Chunks;
            
            if (chunks is null)
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.Chunks)}' is null";
                return;
            }

            if (chunks.Count == 0)
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.Chunks)}' must contain at least 1 element";
                return;
            }

            if (!IsPowerOfTwo(data.AtlasSize))
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.AtlasSize)}' must be power of 2";
                return;
            }

            if (!Enum.IsDefined(typeof(TextureFormat), data.AtlasFormat))
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.AtlasFormat)}' is invalid";
                return;
            }

            _spriteSizeCash.Clear();
            var prevSpriteSizeLog2 = 0;
            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var itemCount = (int) Math.Ceiling(Math.Sqrt(chunk.itemCount));
                var size = itemCount * chunk.spriteSize;
                if (data.AtlasSize < size)
                {
                    result.IsError = true;
                    result.Message = $"'{nameof(data.AtlasSize)}' must be at least {size} to support '{nameof(data.Chunks)}' at index {i}";
                    return;
                }

                if (!_spriteSizeCash.Add(chunk.spriteSize))
                {
                    result.IsError = true;
                    result.Message = $"More than one '{nameof(data.Chunks)}' has '{nameof(chunk.spriteSize)}' with value {chunk.spriteSize}";
                    return;
                }

                var spriteSizeLog2 = (int) Math.Ceiling(Math.Log(chunk.spriteSize, 2));
                if ((_spriteSizeCash.Count > 1) && (spriteSizeLog2 - prevSpriteSizeLog2 != 1))
                {
                    result.IsError = true;
                    result.Message = $"'{nameof(chunk.spriteSize)}' in '{nameof(data.Chunks)}' must create power of 2 sequence";
                    return;
                }

                prevSpriteSizeLog2 = spriteSizeLog2;
            }
        }
        
        private static bool IsPowerOfTwo(int a)
        {
            return (a != 0) && ((a & (a - 1)) == 0);
        }
    }
}