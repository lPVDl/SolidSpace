using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using SpaceSimulator.Editor.Common;
using UnityEngine;

namespace SpaceSimulator.Editor.CodeInspection.NamespaceTool
{
    public class NamespaceToolAsset : ScriptableObject
    {
        [SerializeField] private NamespaceToolConfig _config;

        [Button]
        private void ScanAndLog()
        {
            EditorConsoleUtil.Clear();
            
            var folderScanner = new NamespaceToolFolderScanner();
            var output = new List<NamespaceToolFolderInfo>();
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            
            folderScanner.Scan(projectRoot, _config, output);

            for (var i = 0; i < output.Count; i++)
            {
                var folderInfo = output[i];
                var message = $"'{folderInfo.fullPath}' by regex '{_config.FolderFilters[folderInfo.regexId].regex}' ({folderInfo.regexId})";
                Debug.Log(message);
            }
            
            Debug.Log($"Total folders: {output.Count};");
        }

        [Button]
        private void ExportToNamespaceProvider()
        {
            var folderScanner = new NamespaceToolFolderScanner();
            var output = new List<NamespaceToolFolderInfo>();
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            
            folderScanner.Scan(projectRoot, _config, output);

            var exporter = new NamespaceToolExporter();

            var sharpFile = Path.Combine(projectRoot, "Assembly-CSharp.csproj.DotSettings");
            
            exporter.ExportFoldersForSkip(sharpFile, output.Select(i => i.fullPath));

            var editorFile = Path.Combine(projectRoot, "Assembly-CSharp-Editor.csproj.DotSettings");
            File.Copy(sharpFile, editorFile, true);

            Debug.Log("Done");
        }
        
        [Button]
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}