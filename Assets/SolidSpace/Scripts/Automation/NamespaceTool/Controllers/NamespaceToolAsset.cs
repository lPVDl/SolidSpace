using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
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
            var output = new List<EntityInfo>();
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            folderScanner.Scan(projectRoot, _config, output);

            foreach (var info in output)
            {
                var regex = _config.FolderFilters[info.regexId].regex;
                var message = $"'{info.name}' by regex '{regex}' ({info.regexId})";
                Debug.Log(message);
            }
            
            Debug.Log($"Total folders: {output.Count};");
        }

        [Button]
        private void ScanAssembliesAndLog()
        {
            ConsoleUtil.ClearLog();

            var assemblyUtil = new AssemblyUtil();
            var output = new List<EntityInfo>();
            assemblyUtil.Scan(_config, output);

            foreach (var info in output)
            {
                var regex = _config.AssemblyFilters[info.regexId].regex;
                var fileName = assemblyUtil.AssemblyToFileName(info.name);
                var message = $"'{info.name}' -> '{fileName}' by regex '{regex}' ({info.regexId})";
                Debug.Log(message);
            }
            
            Debug.Log($"Total assemblies: {output.Count}");
        }

        [Button]
        private void OverrideProjDotSettings()
        {
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            var folderScanner = new FolderScanner();
            var entities = new List<EntityInfo>();
            folderScanner.Scan(projectRoot, _config, entities);

            var tempFile = Path.GetTempFileName();
            var dotSettingsWriter = new DotSettingsWriter();
            dotSettingsWriter.Write(tempFile, entities.Select(i => i.name));
            
            var oldFiles = Directory.GetFiles(projectRoot, "*.csproj.DotSettings");
            foreach (var file in oldFiles)
            {
                File.Delete(file);
            }
            
            var assemblyUtil = new AssemblyUtil();
            assemblyUtil.Scan(_config, entities);
            foreach (var assembly in entities)
            {
                var fileName = assemblyUtil.AssemblyToFileName(assembly.name);
                fileName = Path.Combine(projectRoot, fileName);
                File.Copy(tempFile, fileName);
            }
            
            File.Delete(tempFile);

            Debug.Log("Done. Don't forget to restart Rider.");
        }
        
        [Button]
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}