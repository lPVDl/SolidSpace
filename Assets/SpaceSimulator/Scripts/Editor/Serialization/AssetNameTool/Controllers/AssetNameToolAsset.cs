using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using SpaceSimulator.Editor.Common;
using UnityEditor;
using UnityEngine;

namespace SpaceSimulator.Editor.Serialization.AssetNameTool
{
    public class AssetNameToolAsset : ScriptableObject
    {
        [SerializeField] private AssetNameToolConfig _config;

        [Button]
        private void ScanAndLog()
        {
            EditorConsoleUtil.Clear();
            
            var assetGUIDs = AssetDatabase.FindAssets("t:ScriptableObject");
            var activeFilters = _config.Folders.Where(f => f.enabled).ToArray();

            for (var i = 0; i < assetGUIDs.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                
                for (var j = 0; j < activeFilters.Length; j++)
                {
                    var filter = activeFilters[j];

                    if (Regex.IsMatch(assetPath, filter.scannerRegex))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                        var objName = obj.GetType().ToString();

                        var newName = Regex.Replace(objName, filter.nameRegex, filter.nameSubstitution);

                        Debug.Log($"'{assetPath}' by ({j}); type: '{objName}'; newName : '{newName}'");
                    }
                }
            }
        }
    }
}