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
        
        [SerializeField] private PlaygroundToolConfig _toolConfig;
        [SerializeField] private Color _gizmosColor;
    }
}