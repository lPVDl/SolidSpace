using System;
using System.Collections.Generic;

namespace SolidSpace.Entities.Rendering.Atlases
{
    public class AtlasConfigValidator : IDataValidator<AtlasConfig>
    {
        private readonly HashSet<int> _spriteSizeCash;

        public AtlasConfigValidator()
        {
            _spriteSizeCash = new HashSet<int>();
        }
        
        public string Validate(AtlasConfig data)
        {
            var chunks = data.Chunks;
            
            if (chunks is null)
            {
                return string.Empty;
            }

            if (chunks.Count == 0)
            {
                return $"'{nameof(data.Chunks)}' must contain at least 1 element";
            }

            if (!IsPowerOfTwo(data.AtlasSize))
            {
                return $"'{nameof(data.AtlasSize)}' must be power of 2";
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
                    return $"'{nameof(data.AtlasSize)}' must be at least {size} to support '{nameof(data.Chunks)}' at index {i}";
                }

                if (!_spriteSizeCash.Add(chunk.spriteSize))
                {
                    return $"More than one '{nameof(data.Chunks)}' has '{nameof(chunk.spriteSize)}' with value {chunk.spriteSize}";
                }

                var spriteSizeLog2 = (int) Math.Ceiling(Math.Log(chunk.spriteSize, 2));
                if ((_spriteSizeCash.Count > 1) && (spriteSizeLog2 - prevSpriteSizeLog2 != 1))
                {
                    return $"'{nameof(chunk.spriteSize)}' in '{nameof(data.Chunks)}' must create power of 2 sequence";
                }

                prevSpriteSizeLog2 = spriteSizeLog2;
            }

            return string.Empty;
        }
        
        private static bool IsPowerOfTwo(int a)
        {
            return (a != 0) && ((a & (a - 1)) == 0);
        }
    }
}