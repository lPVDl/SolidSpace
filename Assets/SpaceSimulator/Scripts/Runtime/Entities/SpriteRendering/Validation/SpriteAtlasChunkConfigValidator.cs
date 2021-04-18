namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasChunkConfigValidator : IValidator<SpriteAtlasChunkConfig>
    {
        public void Validate(SpriteAtlasChunkConfig data, ValidationResult result)
        {
            var minSpriteSize = 1 << SpriteAtlasIndexManager.MinSpritePower;
            if (data.spriteSize < minSpriteSize)
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.spriteSize)}' must be equal or more than {minSpriteSize}";
                return;
            }

            if (!IsPowerOfTwo(data.spriteSize))
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.spriteSize)}' must be power of 2";
                return;
            }

            if (!IsPowerOfFour(data.itemCount))
            {
                result.IsError = true;
                result.Message = $"'{nameof(data.itemCount)}' must be power of 4";
            }
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