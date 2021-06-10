using SolidSpace.DataValidation;

namespace SolidSpace.Playground.Core
{
    [InspectorDataValidator]
    public class PlaygroundToolConfigValidator : IDataValidator<PlaygroundToolConfig>
    {
        public string Validate(PlaygroundToolConfig data)
        {
            if (data.Icon is null)
            {
                return $"{nameof(data.Icon)} is null";
            }

            if (string.IsNullOrEmpty(data.Name))
            {
                return $"'{nameof(data.Name)}' must be not empty string";
            }

            return string.Empty;
        }
    }
}