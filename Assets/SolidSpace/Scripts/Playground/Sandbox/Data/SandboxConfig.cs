using System;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.Data
{
    [Serializable]
    internal class SandboxConfig
    {
        public UIPrefab ToolWindowPrefab => _toolWindowPrefab;
        public UIPrefab ToolIconPrefab => _toolIconPrefab;
        public UIPrefab<ICheckedButton> CheckedButtonPrefab => _checkedButtonPrefab;
        
        [SerializeField] private UIPrefab _toolWindowPrefab;
        [SerializeField] private UIPrefab _toolIconPrefab;

        [SerializeField] private UIPrefab<ICheckedButton> _checkedButtonPrefab;
    }
}