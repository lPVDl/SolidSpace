using System;
using System.Collections.Generic;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    [Serializable]
    internal class SandboxConfig
    {
        public UIPrefab<IToolButtonView> CheckedButtonPrefab => _checkedButtonPrefab;
        public UIPrefab<IToolWindowView> ToolWindowPrefab => _toolWindowPrefab;

        [SerializeField] private UIPrefab<IToolButtonView> _checkedButtonPrefab;
        [SerializeField] private UIPrefab<IToolWindowView> _toolWindowPrefab;
    }
}