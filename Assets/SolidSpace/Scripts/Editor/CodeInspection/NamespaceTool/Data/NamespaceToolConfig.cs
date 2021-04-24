using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Editor.CodeInspection.NamespaceTool
{
    [System.Serializable]
    public class NamespaceToolConfig
    {
        public string ScriptsRoot => _scriptsRoot;
        public IReadOnlyList<NamespaceToolFolderFilter> FolderFilters => _folderFilters;

        [SerializeField] private string _scriptsRoot;
        
        [SerializeField] private List<NamespaceToolFolderFilter> _folderFilters;
    }
}