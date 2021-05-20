using System.IO;
using SolidSpace.DataValidation;
using UnityEngine;

namespace SolidSpace.Editor.Automation.NamespaceTool
{
    internal class ConfigValidator : IDataValidator<Config>
    {
        private readonly string _projectRoot;
        
        public ConfigValidator()
        {
            _projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
        }
        
        public string Validate(Config data)
        {
            if (data.ScriptsRoot is null)
            {
                return $"{nameof(data.ScriptsRoot)} is null or empty";
            }

            var scriptsPath = Path.Combine(_projectRoot, data.ScriptsRoot);
            if (!Directory.Exists(scriptsPath))
            {
                return $"Path '{scriptsPath}' is invalid";
            }

            return string.Empty;
        }
    }
}