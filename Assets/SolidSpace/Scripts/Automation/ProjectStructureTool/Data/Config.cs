using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Automation.ProjectStructureTool
{
    [Serializable]
    internal class Config
    {
        public string ScanRoot => _scanRoot;
        public IReadOnlyList<FilterInfo> BlackListFilters => _blacklistFilters;
        public string ExportPath => _exportPath;
        
        [SerializeField] private string _scanRoot;
        [SerializeField] private List<FilterInfo> _blacklistFilters;
        [SerializeField] private string _exportPath;
    }
}