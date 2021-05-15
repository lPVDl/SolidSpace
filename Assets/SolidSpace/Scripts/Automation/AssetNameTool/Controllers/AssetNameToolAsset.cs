using System.Collections.Generic;
using Sirenix.OdinInspector;
using SolidSpace.EditorUtilities;
using UnityEngine;

namespace SolidSpace.Automation.AssetNameTool
{
    public class AssetNameToolAsset : ScriptableObject
    {
        [SerializeField] private Config _config;

        [Button]
        private void ScanAndLog()
        {
            ConsoleUtil.ClearLog();
            
            var processor = new FileProcessor();
            var files = new List<FileInfo>();
            processor.Process(_config, files);
            
            foreach (var file in files)
            {
                Debug.Log($"({file.typeName}) ({file.foundByRegexId}) '{file.originalPath}' -> '{file.modifiedPath}'");
            }
        }

        [Button]
        private void RenameAssets()
        {
            ConsoleUtil.ClearLog();
            
            var processor = new FileProcessor();
            var renamer = new FileNameUtil();
            var files = new List<FileInfo>();
            
            processor.Process(_config, files);
            renamer.Rename(files);
        }
    }
}