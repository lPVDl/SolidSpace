using System.IO;
using UnityEngine;

namespace SpaceSimulator.Editor.CodeInspection.NamespaceTool
{
    public class NamespaceToolConfigValidator : IValidator<NamespaceToolConfig>
    {
        private readonly string _projectRoot;
        
        public NamespaceToolConfigValidator()
        {
            _projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
        }
        
        public string Validate(NamespaceToolConfig data)
        {
            if (data.ScriptsRoot is null)
            {
                return $"{nameof(data.ScriptsRoot)} is null or empty";
            }

            var scriptsPath = Path.Combine(_projectRoot, data.ScriptsRoot);
            if (!Directory.Exists(scriptsPath))
            {
                return $"'Path {scriptsPath}' is invalid";
            }

            return string.Empty;
        }
    }
}