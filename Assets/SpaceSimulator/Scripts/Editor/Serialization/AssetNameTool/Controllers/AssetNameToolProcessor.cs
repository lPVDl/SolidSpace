using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SpaceSimulator.Editor.Serialization.AssetNameTool
{
    public class AssetNameToolProcessor
    {
        public void Process(AssetNameToolConfig config, ICollection<AssetNameToolFile> result)
        {
            result.Clear();
            
            var assetGUIDs = AssetDatabase.FindAssets("t:ScriptableObject");
            var filters = config.Folders;
            
            for (var i = 0; i < assetGUIDs.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

                for (var j = 0; j < filters.Count; j++)
                {
                    var filter = filters[j];

                    if (!filter.enabled)
                    {
                        continue;
                    }
                    
                    if (!Regex.IsMatch(assetPath, filter.scannerRegex))
                    {
                        continue;
                    }

                    var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    var typeName = obj.GetType().ToString();
                    var oldName = Path.GetFileName(assetPath);
                    var newName = Regex.Replace(typeName, filter.nameRegex, filter.nameSubstitution);
                    var assetRoot = assetPath.Substring(0, assetPath.LastIndexOf(oldName));

                    result.Add( new AssetNameToolFile
                    {
                        foundByRegexId = j,
                        typeName = typeName,
                        originalPath = assetPath,
                        modifiedPath = assetRoot + newName
                    });
                }
            }
        }
    }
}