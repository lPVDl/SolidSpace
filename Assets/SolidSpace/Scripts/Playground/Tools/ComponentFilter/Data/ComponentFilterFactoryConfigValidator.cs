using SolidSpace.DataValidation;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [InspectorDataValidator]
    public class ComponentFilterFactoryConfigValidator : IDataValidator<ComponentFilterFactoryConfig>
    {
        public string Validate(ComponentFilterFactoryConfig data)
        {
            if (data.FilterRegex is null)
            {
                return $"{nameof(data.FilterRegex)} is null";
            }
            
            if (data.NameRegex is null)
            {
                return $"{nameof(data.NameRegex)} is null";
            }

            if (data.NameSubstitution is null)
            {
                return $"{nameof(data.NameSubstitution)} is null";
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.FilterRegex), data.FilterRegex, out var message))
            {
                return message;
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.NameRegex), data.NameRegex, out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}