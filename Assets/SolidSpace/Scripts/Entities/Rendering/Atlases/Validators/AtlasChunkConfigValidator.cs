namespace SolidSpace.Entities.Rendering.Atlases
{
    public class AtlasChunkConfigValidator : IDataValidator<AtlasChunkConfig>
    {
        public string Validate(AtlasChunkConfig data)
        {
            const int minSpriteSize = 1 << AtlasIndexManager.MinSpritePower;
            
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