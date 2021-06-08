using System;
using System.Collections.Generic;
using SolidSpace.DataValidation;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxConfigValidator : IDataValidator<SandboxConfig>
    {
        private readonly EPlaygroundTool[] _allTools;
        private readonly HashSet<EPlaygroundTool> _toolHashset;
        
        public SandboxConfigValidator()
        {
            _allTools = (EPlaygroundTool[]) Enum.GetValues(typeof(EPlaygroundTool));
            _toolHashset = new HashSet<EPlaygroundTool>();
        }
        
        public string Validate(SandboxConfig data)
        {
            if (data.CheckedButtonPrefab is null)
            {
                return $"'{nameof(data.CheckedButtonPrefab)}' is null";
            }

            if (data.ToolWindowPrefab is null)
            {
                return $"'{nameof(data.ToolWindowPrefab)}' is null";
            }

            if (data.ToolIcons is null)
            {
                return $"'{nameof(data.ToolIcons)}' is null";
            }
            
            _toolHashset.Clear();
            var toolIcons = data.ToolIcons;
            for (var i = 0; i < toolIcons.Count; i++)
            {
                var tool = toolIcons[i].tool;
                if (!_toolHashset.Add(tool))
                {
                    return $"Icon for '{tool}' is duplicated in '{nameof(data.ToolIcons)}'";
                }
            }

            if (_toolHashset.Count != _allTools.Length)
            {
                for (var i = 0; i < _allTools.Length; i++)
                {
                    var tool = _allTools[i];
                    
                    if (_toolHashset.Add(tool))
                    {
                        return $"'{nameof(data.ToolIcons)}' is missing icon for '{tool}'";
                    }
                }
            }
            
            return string.Empty;
        }
    }
}