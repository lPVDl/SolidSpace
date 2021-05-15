using System.IO;
using SolidSpace.DataValidation;
using UnityEngine;

namespace SolidSpace.Automation.ProjectStructureTool
{
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

            var appRoot = Application.dataPath;
            var projectRoot = appRoot.Substring(0, appRoot.Length - 7);
            var directory = Path.Combine(projectRoot, data.ScanRoot);
            if (!Directory.Exists(directory))
            {
                return $"{nameof(data.ScanRoot)}, directory '{directory}' does not exist";
            }

            var file = Path.Combine(projectRoot, data.ExportPath);
            if (!File.Exists(file))
            {
                return $"{nameof(data.ExportPath)}, file '{file}' does not exist";
            }

            return string.Empty;
        }
    }
}