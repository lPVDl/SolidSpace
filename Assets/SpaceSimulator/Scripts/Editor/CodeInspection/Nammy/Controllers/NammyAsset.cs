using System.Collections.Generic;
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

        [HorizontalGroup("Control"), Button("Regex Help")]
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}