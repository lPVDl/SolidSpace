using System.Collections.Generic;
using Sirenix.OdinInspector;
using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Editor.Automation.ChangelogTool
{
    [System.Serializable]
    public class ChangelogToolConfig
    {
        public IReadOnlyList<RegexPattern> Blacklist => _blacklist;
        public IReadOnlyList<RegexPatternSubstitution> Converters => _converters;
        
        [SerializeField, TableList] private List<RegexPattern> _blacklist;
        [SerializeField, TableList] private List<RegexPatternSubstitution> _converters;
    }
}