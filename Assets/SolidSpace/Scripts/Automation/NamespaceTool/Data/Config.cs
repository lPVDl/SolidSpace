using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SolidSpace.Automation.NamespaceTool
{
    [System.Serializable]
    internal class Config
    {
        public string ScriptsRoot => _scriptsRoot;
        public IReadOnlyList<FilterInfo> FolderFilters => _folderFilters;
        public IReadOnlyList<FilterInfo> AssemblyFilters => _assemblyFilters;

        [SerializeField] private string _scriptsRoot;
        
        [SerializeField, TableList] private List<FilterInfo> _folderFilters;

        [SerializeField, TableList] private List<FilterInfo> _assemblyFilters;
    }
}