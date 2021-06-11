using System;
using SolidSpace.Playground.Core;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Eraser
{
    [Serializable]
    public class EraserToolConfig
    {
        public PlaygroundToolConfig ToolConfig => _toolConfig;
        public Color GizmosColor => _gizmosColor;
        public string ComponentFilterRegex => _componentFilterRegex;
        public string ComponentNameRegex => _componentNameRegex;
        public string ComponentNameSubstitution => _componentNameSubstitution;

        [SerializeField] private PlaygroundToolConfig _toolConfig;
        [SerializeField] private Color _gizmosColor;
        [SerializeField] private string _componentFilterRegex;
        [SerializeField] private string _componentNameRegex;
        [SerializeField] private string _componentNameSubstitution;
    }
}