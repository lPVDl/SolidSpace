using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSimulator.Editor.CodeFlow
{
    [System.Serializable]
    public class NammyConfig
    {
        public string ScriptsRoot => _scriptsRoot;
        public IReadOnlyList<NammyFolderFilter> FolderFilters => _folderFilters;

        [SerializeField] private string _scriptsRoot;
        
        [SerializeField, TableList] private List<NammyFolderFilter> _folderFilters;
    }
}