using System;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [Serializable]
    public class ComponentFilterWindowConfig
    {
        public string ComponentNameRegex => _componentNameRegex;
        public string ComponentNameSubstitution => _componentNameSubstitution;
        
        [SerializeField] private string _componentNameRegex;
        [SerializeField] private string _componentNameSubstitution;
    }
}