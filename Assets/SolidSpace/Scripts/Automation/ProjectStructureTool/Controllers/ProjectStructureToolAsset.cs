using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SolidSpace.Automation.ProjectStructureTool
{
    internal class ProjectStructureToolAsset : ScriptableObject
    {
        [SerializeField] private Config _config;

        [Button]
        private void ScanAndLog()
        {
            var console = new UnityConsole(true);
            var writer = new FileWriter();
            writer.Write(_config, console);
        }

        [Button]
        private void ScanAndExport()
        {
            var dataPath = Application.dataPath;
            var projectRoot = dataPath.Substring(0, dataPath.Length - 7);
            var exportPath = Path.Combine(projectRoot, _config.ExportPath);

            using var console = new FileConsole(exportPath, true);
            var writer = new FileWriter();
            writer.Write(_config, console);
            
            Debug.Log("Done.");
        }
    }
}