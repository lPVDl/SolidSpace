using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using SolidSpace.DataValidation;
using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Rendering.Atlases
{
    [InspectorDataValidator]
    internal class TextureAtlasConfigValidator : IDataValidator<TextureAtlasConfig>
    {
        private readonly HashSet<int> _spriteSizeCash;

        public TextureAtlasConfigValidator()
        {
            _spriteSizeCash = new HashSet<int>();
        }
        
        public string Validate(TextureAtlasConfig data)
        {
            var chunks = data.Chunks;
            
            if (chunks is null)
            {
                return $"{nameof(data.Chunks)} is null";
            }

            if (chunks.Count == 0)
            {
                return $"{nameof(data.Chunks)} must contain at least 1 element";
            }

            if (!BinaryMath.IsPowerOfTwo(data.AtlasSize))
            {
                return $"{nameof(data.AtlasSize)} must be power of 2";
            }

            if (data.AtlasSize > AtlasMath.Max2DAtlasSize)
            {
                return $"{nameof(data.AtlasSize)} must be <= {AtlasMath.Max2DAtlasSize}";
            }

            _spriteSizeCash.Clear();
            var previousSizeLog2 = 0;
            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var itemCount = (int) Math.Ceiling(Math.Sqrt(chunk.itemCount));
                var size = itemCount * chunk.itemSize;
                if (data.AtlasSize < size)
                {
                    return $"{nameof(data.AtlasSize)} must be at least {size} to support {nameof(data.Chunks)} at index {i}";
                }

                if (!_spriteSizeCash.Add(chunk.itemSize))
                {
                    return $"More than one {nameof(data.Chunks)} has {nameof(chunk.itemSize)} with value {chunk.itemSize}";
                }

                var currentSizeLog2 = (int) Math.Ceiling(Math.Log(chunk.itemSize, 2));
                if ((_spriteSizeCash.Count > 1) && (currentSizeLog2 - previousSizeLog2 != 1))
                {
                    return $"{nameof(chunk.itemSize)} in {nameof(data.Chunks)} must create power of 2 sequence";
                }

                previousSizeLog2 = currentSizeLog2;
            }

            return string.Empty;
        }
    }
}