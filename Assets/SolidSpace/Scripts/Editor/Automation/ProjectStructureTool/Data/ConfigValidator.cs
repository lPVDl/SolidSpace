using System.IO;
using SolidSpace.DataValidation;
using SolidSpace.IO.Editor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.ProjectStructureTool
{
    [InspectorDataValidator]
    internal class ConfigValidator : IDataValidator<Config>
    {
        public string Validate(Config data)
        {
            if (data.ScanRoot is null)
            {
                return $"{nameof(data.ScanRoot)} is null";
            }

            if (data.ExportPath is null)
            {
                return $"{nameof(data.ExportPath)} is null";
            }

            var directory = EditorPath.Combine(EditorPath.ProjectRoot, data.ScanRoot);
            if (!Directory.Exists(directory))
            {
                return $"{nameof(data.ScanRoot)}, directory '{directory}' does not exist";
            }

            var file = EditorPath.Combine(EditorPath.ProjectRoot, data.ExportPath);
            if (!File.Exists(file))
            {
                return $"{nameof(data.ExportPath)}, file '{file}' does not exist";
            }

            return string.Empty;
        }
    }
}