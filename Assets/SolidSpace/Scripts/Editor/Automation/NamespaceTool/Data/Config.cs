using System.Collections.Generic;
using Sirenix.OdinInspector;
using SolidSpace.IO.Editor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.NamespaceTool
{
    [System.Serializable]
    internal class Config
    {
        public EditorFolderPath ScanPath => _scanPath;
        public IReadOnlyList<FilterInfo> FolderFilters => _folderFilters;
        public IReadOnlyList<FilterInfo> AssemblyFilters => _assemblyFilters;

        [SerializeField] private EditorFolderPath _scanPath;
        [SerializeField, TableList] private List<FilterInfo> _folderFilters;
        [SerializeField, TableList] private List<FilterInfo> _assemblyFilters;
    }
}