using System.Collections.Generic;
using SolidSpace.DataValidation;

namespace SolidSpace.GameCycle
{
    [InspectorDataValidator]
    public class ControllerGroupValidator : IDataValidator<ControllerGroup>
    {
        private readonly HashSet<string> _names;

        public ControllerGroupValidator()
        {
            _names = new HashSet<string>();
        }
        
        public string Validate(ControllerGroup data)
        {
            if (string.IsNullOrEmpty(data.Name))
            {
                return $"'{nameof(data.Name)}' is null or empty";
            }

            if (data.Controllers is null)
            {
                return $"'{nameof(data.Controllers)}' is null";
            }
            
            _names.Clear();

            for (var i = 0; i < data.Controllers.Count; i++)
            {
                var name = data.Controllers[i];
                if (string.IsNullOrEmpty(name))
                {
                    return $"Null or empty string at ({i}) in '{nameof(data.Controllers)}'";
                }

                if (!_names.Add(name))
                {
                    return $"Duplicated item '{name}' in {nameof(data.Controllers)}";
                }
            }

            return string.Empty;
        }
    }
}