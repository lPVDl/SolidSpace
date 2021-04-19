using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSimulator.Editor.CodeFlow
{
    public class NammyAsset : ScriptableObject
    {
        [SerializeField] private NammyConfig _config;

        [HorizontalGroup("Control"), Button("Scan & Log")]
        private void ScanAndLog()
        {
            var folderScanner = new NammyFolderScanner();
            var output = new List<NammyFolderInfo>();
            
            folderScanner.Scan(_config, output);

            for (var i = 0; i < output.Count; i++)
            {
                var folderInfo = output[i];
                var message = $"'{folderInfo.fullPath}' by regex '{_config.FolderFilters[folderInfo.regexId].regex}' ({folderInfo.regexId})";
                Debug.Log(message);
            }
            
            Debug.Log($"Total folders: {output.Count}; Editor folders: {output.Count(o => o.isEditor)}");
        }

        [HorizontalGroup("Export"), Button("Export to NamespaceProvider")]
        private void ExportToNamespaceProvider()
        {
            var folderScanner = new NammyFolderScanner();
            var output = new List<NammyFolderInfo>();
            
            folderScanner.Scan(_config, output);

            var exporter = new NammyExporter();

            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            
            exporter.ExportFoldersForSkip(Path.Combine(projectRoot, "Assembly-CSharp.csproj.DotSettings"),
                output.Where(o => !o.isEditor).Select(i => i.fullPath));
            
            exporter.ExportFoldersForSkip(Path.Combine(projectRoot, "Assembly-CSharp-Editor.csproj.DotSettings"),
                output.Where(o => o.isEditor).Select(i => i.fullPath));
            
            Debug.Log("Done");
        }
        
        [HorizontalGroup("Control"), Button("Regex Help")]
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}