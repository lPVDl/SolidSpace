using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceSimulator.Editor.Serialization.AssetNameTool
{
    public class AssetNameToolRenamer
    {
        private readonly HashSet<string> _textHash;

        public AssetNameToolRenamer()
        {
            _textHash = new HashSet<string>();
        }
        
        public void Rename(IReadOnlyList<AssetNameToolFile> files)
        {
            _textHash.Clear();
            
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                
                if (!CheckRequiresRenaming(file))
                {
                    _textHash.Add(file.modifiedPath.ToLowerInvariant());
                    continue;
                }
                
                var guid = AssetDatabase.AssetPathToGUID(file.modifiedPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    Debug.LogError($"Abort. Name '{file.modifiedPath}' is occupied.");
                    return;
                }

                if (!_textHash.Add(file.modifiedPath.ToLowerInvariant()))
                {
                    Debug.LogError($"Abort. Name '{file.modifiedPath}' will be duplicated.");
                    return;
                }

                var message = AssetDatabase.ValidateMoveAsset(file.originalPath, file.modifiedPath);
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.LogError($"Abort. ValidateMoveAsset: '{message}'");
                    return;
                }
            }

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (CheckRequiresRenaming(file))
                {
                    continue;
                }

                var message = AssetDatabase.RenameAsset(file.originalPath, file.modifiedPath);
                Debug.Log($"Renamed '{file.originalPath}' -> '{file.modifiedPath}'; extraInfo: '{message}'");
            }
        }

        private bool CheckRequiresRenaming(AssetNameToolFile file)
        {
            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            return string.Compare(file.originalPath, file.modifiedPath, comparison) != 0;
        }
    }
}