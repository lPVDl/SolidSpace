using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SolidSpace.Editor.CodeInspection.NamespaceTool
{
    [System.Serializable]
    public class NamespaceToolConfig
    {
        public string ScriptsRoot => _scriptsRoot;
        public IReadOnlyList<NamespaceToolFilter> FolderFilters => _folderFilters;
        public IReadOnlyList<NamespaceToolFilter> AssemblyFilters => _assemblyFilters;

        [SerializeField] private string _scriptsRoot;
        
        [SerializeField, TableList] private List<NamespaceToolFilter> _folderFilters;

        [SerializeField, TableList] private List<NamespaceToolFilter> _assemblyFilters;
    }
}