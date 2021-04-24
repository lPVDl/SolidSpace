using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpaceSimulator.Editor.Common;
using UnityEngine;

namespace SpaceSimulator.Editor.Serialization.AssetNameTool
{
    public class AssetNameToolAsset : ScriptableObject
    {
        [SerializeField] private AssetNameToolConfig _config;

        [Button]
        private void ScanAndLog()
        {
            EditorConsoleUtil.ClearLog();
            
            var processor = new AssetNameToolProcessor();
            var files = new List<AssetNameToolFile>();
            processor.Process(_config, files);
            
            foreach (var file in files)
            {
                Debug.Log($"({file.typeName}) ({file.foundByRegexId}) '{file.originalPath}' -> '{file.modifiedPath}'");
            }
        }

        [Button]
        private void RenameAssets()
        {
            EditorConsoleUtil.ClearLog();
            
            var processor = new AssetNameToolProcessor();
            var renamer = new AssetNameToolRenamer();
            var files = new List<AssetNameToolFile>();
            
            processor.Process(_config, files);
            renamer.Rename(files);
        }
    }
}