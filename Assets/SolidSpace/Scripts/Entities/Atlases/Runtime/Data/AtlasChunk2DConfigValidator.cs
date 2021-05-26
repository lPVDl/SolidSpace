using SolidSpace.DataValidation;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    internal class AtlasChunk2DConfigValidator : IDataValidator<AtlasChunk2DConfig>
    {
        public string Validate(AtlasChunk2DConfig data)
        {
            if (data.itemSize < AtlasMath.Min2DEntitySize)
            {
                return $"'{nameof(data.itemSize)}' must be equal or more than {AtlasMath.Min2DEntitySize}";
            }

            if (!BinaryMath.IsPowerOfTwo(data.itemSize))
            {
                return $"'{nameof(data.itemSize)}' must be power of 2";
            }

            if (!BinaryMath.IsPowerOfFour(data.itemCount))
            {
                return $"'{nameof(data.itemCount)}' must be power of 4";
            }

            return string.Empty;
        }
    }
}