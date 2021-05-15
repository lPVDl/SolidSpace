using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using SolidSpace.EditorUtilities;
using UnityEngine;

namespace SolidSpace.Automation.NamespaceTool
{
    internal class NamespaceToolAsset : ScriptableObject
    {
        [SerializeField] private Config _config;

        [Button]
        private void ScanFoldersAndLog()
        {
            ConsoleUtil.ClearLog();
            
            var folderScanner = new FolderScanner();
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            var folders = folderScanner.Scan(projectRoot, _config);

            foreach (var info in folders)
            {
                var regex = _config.FolderFilters[info.regexId].regex;
                var message = $"'{info.name}' by regex '{regex}' ({info.regexId})";
                Debug.Log(message);
            }
            
            Debug.Log($"Total folders: {folders.Count};");
        }

        [Button]
        private void ScanAssembliesAndLog()
        {
            ConsoleUtil.ClearLog();

            var assemblyUtil = new AssemblyUtil();
            var assemblies = assemblyUtil.Scan(_config);

            foreach (var assembly in assemblies)
            {
                var regex = _config.AssemblyFilters[assembly.regexId].regex;
                var fileName = assemblyUtil.AssemblyToFileName(assembly.name);
                var message = $"'{assembly.name}' -> '{fileName}' by regex '{regex}' ({assembly.regexId}):";
                
                Debug.Log(message);
                
                foreach (var folder in assembly.folders)
                {
                    Debug.Log($"\t{folder}");
                }
            }

            var folderCount = assemblies.SelectMany(a => a.folders).Count();
            Debug.Log($"Total assemblies: {assemblies.Count}; Total folders: {folderCount}");
        }

        [Button]
        private void OverrideProjDotSettings()
        {
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            var oldFiles = Directory.GetFiles(projectRoot, "*.csproj.DotSettings");
            foreach (var file in oldFiles)
            {
                File.Delete(file);
            }
            
            var folderScanner = new FolderScanner();
            var assemblyUtil = new AssemblyUtil();
            var dotSettingsWriter = new DotSettingsWriter();
            var folders = folderScanner.Scan(projectRoot, _config);
            var folderNames = new HashSet<string>(folders.Select(f => f.name));
            var assemblies = assemblyUtil.Scan(_config);
            foreach (var assembly in assemblies)
            {
                var assemblyPath = assemblyUtil.AssemblyToFileName(assembly.name);
                var foldersToSkip = assembly.folders.Where(f => folderNames.Contains(f));
                dotSettingsWriter.Write(assemblyPath, foldersToSkip);
            }

            Debug.Log("Done. Don't forget to restart Rider.");
        }

        [Button]
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}