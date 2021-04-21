using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSimulator.Editor.CodeInspection.NamespaceTool
{
    [System.Serializable]
    public class NamespaceToolConfig
    {
        public string ScriptsRoot => _scriptsRoot;
        public IReadOnlyList<NamespaceToolFolderFilter> FolderFilters => _folderFilters;

        [SerializeField] private string _scriptsRoot;
        
        [SerializeField, TableList] private List<NamespaceToolFolderFilter> _folderFilters;
    }
}