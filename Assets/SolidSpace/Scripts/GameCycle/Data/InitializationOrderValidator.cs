using System;
using System.Collections.Generic;
using SolidSpace.DataValidation;

namespace SolidSpace.GameCycle
{
    [InspectorDataValidator]
    public class InitializationOrderValidator : IDataValidator<InitializationOrder>
    {
        private readonly HashSet<string> _controllerNames;
        private readonly HashSet<string> _groupNames;
        private readonly Dictionary<string, Type> _nameToType;
        private readonly Type _initializableType;

        public InitializationOrderValidator()
        {
            _controllerNames = new HashSet<string>();
            _groupNames = new HashSet<string>();
            _nameToType = new Dictionary<string, Type>();
            _initializableType = typeof(IInitializable);
        }
        
        public string Validate(InitializationOrder data)
        {
            if (data.Groups is null)
            {
                return $"'{nameof(data.Groups)}' is null";
            }

            _groupNames.Clear();
            _controllerNames.Clear();

            for (var i = 0; i < data.Groups.Count; i++)
            {
                var group = data.Groups[i];
                if (!_groupNames.Add(group.Name))
                {
                    return $"Group name '{group.Name}' is duplicated";
                }
                
                if (group.Controllers is null)
                {
                    return $"{nameof(group.Controllers)} is null at group ({i}) '{group.Name}'";
                }
                
                for (var j = 0; j < group.Controllers.Count; j++)
                {
                    var controllerName = group.Controllers[j];
                    if (!_controllerNames.Add(controllerName))
                    {
                        return $"Controller name '{controllerName}' is duplicated";
                    }

                    if (_nameToType.ContainsKey(controllerName))
                    {
                        continue;
                    }

                    var type = Type.GetType(controllerName);
                    if (type is null)
                    {
                        return $"Type for controller with name '{controllerName}' was not found";
                    }

                    if (!_initializableType.IsAssignableFrom(type))
                    {
                        return $"Controller '{controllerName}' does not implement {nameof(IInitializable)}";
                    }
                    
                    _nameToType.Add(controllerName, type);
                }
            }
            
            return string.Empty;
        }
    }
}