using SolidSpace.DataValidation;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [InspectorDataValidator]
    public class ComponentFilterMasterConfigValidator : IDataValidator<ComponentFilterMasterConfig>
    {
        public string Validate(ComponentFilterMasterConfig data)
        {
            if (data.FilterRegex is null)
            {
                return $"{nameof(data.FilterRegex)} is null";
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.FilterRegex), data.FilterRegex, out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}