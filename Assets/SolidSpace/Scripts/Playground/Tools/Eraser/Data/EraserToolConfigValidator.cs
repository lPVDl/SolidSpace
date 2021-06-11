using SolidSpace.DataValidation;

namespace SolidSpace.Playground.Tools.Eraser
{
    [InspectorDataValidator]
    public class EraserToolConfigValidator : IDataValidator<EraserToolConfig>
    {
        public string Validate(EraserToolConfig data)
        {
            if (data.ComponentFilterRegex is null)
            {
                return $"{nameof(data.ComponentFilterRegex)} is null";
            }
        
            if (data.ComponentNameRegex is null)
            {
                return $"{nameof(data.ComponentNameRegex)} is null";
            }

            if (data.ComponentNameSubstitution is null)
            {
                return $"{nameof(data.ComponentNameSubstitution)} is null";
            }
            
            if (!ValidationUtil.RegexIsValid(nameof(data.ComponentFilterRegex), data.ComponentFilterRegex, out var message))
            {
                return message;
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.ComponentNameRegex), data.ComponentNameRegex, out message))
            {
                return message;
            }
            
            return string.Empty;
        }
    }
}