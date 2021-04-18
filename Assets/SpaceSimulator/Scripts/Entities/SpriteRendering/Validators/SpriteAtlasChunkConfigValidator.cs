using SpaceSimulator.Interfaces;

namespace SpaceSimulator.Entities.SpriteRendering
{
    public class SpriteAtlasChunkConfigValidator : IValidator<SpriteAtlasChunkConfig>
    {
        public string Validate(SpriteAtlasChunkConfig data)
        {
            const int minSpriteSize = 1 << SpriteAtlasIndexManager.MinSpritePower;
            
            if (data.spriteSize < minSpriteSize)
            {
                return $"'{nameof(data.spriteSize)}' must be equal or more than {minSpriteSize}";
            }

            if (!IsPowerOfTwo(data.spriteSize))
            {
                return $"'{nameof(data.spriteSize)}' must be power of 2";
            }

            if (!IsPowerOfFour(data.itemCount))
            {
                return $"'{nameof(data.itemCount)}' must be power of 4";
            }

            return string.Empty;
        }

        private static bool IsPowerOfTwo(int a)
        {
            return (a != 0) && ((a & (a - 1)) == 0);
        }

        private static bool IsPowerOfFour(int a)
        {
            if (!IsPowerOfTwo(a))
            {
                return false;
            }

            return (a & 0x55555555) != 0;
        }
    }
}