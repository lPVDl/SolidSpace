using SolidSpace.DataValidation;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [InspectorDataValidator]
    public class ComponentFilterWindowConfigValidator : IDataValidator<ComponentFilterWindowConfig>
    {
        public string Validate(ComponentFilterWindowConfig data)
        {
            if (data.ComponentNameRegex is null)
            {
                return $"{nameof(data.ComponentNameRegex)} is null";
            }

            if (data.ComponentNameSubstitution is null)
            {
                return $"{nameof(data.ComponentNameSubstitution)} is null";
            }
            
            if (!ValidationUtil.RegexIsValid(nameof(data.ComponentNameRegex), data.ComponentNameRegex, out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}