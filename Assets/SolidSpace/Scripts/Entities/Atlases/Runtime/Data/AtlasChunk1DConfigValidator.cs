using SolidSpace.DataValidation;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    [InspectorDataValidator]
    internal class AtlasChunk1DConfigValidator : IDataValidator<AtlasChunk1DConfig>
    {
        public string Validate(AtlasChunk1DConfig data)
        {
            if (data.itemSize < AtlasMath.Min1DEntitySize)
            {
                return $"'{nameof(data.itemSize)}' must be equal or more than {AtlasMath.Min1DEntitySize}";
            }

            if (!BinaryMath.IsPowerOfTwo(data.itemSize))
            {
                return $"'{nameof(data.itemSize)}' must be power of 2";
            }

            if (!BinaryMath.IsPowerOfTwo(data.itemCount))
            {
                return $"'{nameof(data.itemCount)}' must be power of 2";
            }

            return string.Empty;
        }
    }
}