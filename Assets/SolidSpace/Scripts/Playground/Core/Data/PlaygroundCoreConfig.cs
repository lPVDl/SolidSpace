using System;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    [Serializable]
    internal class PlaygroundCoreConfig
    {
        public UIPrefab<IToolButtonView> CheckedButtonPrefab => _checkedButtonPrefab;
        public UIPrefab<IToolWindowView> ToolWindowPrefab => _toolWindowPrefab;

        [SerializeField] private UIPrefab<IToolButtonView> _checkedButtonPrefab;
        [SerializeField] private UIPrefab<IToolWindowView> _toolWindowPrefab;
    }
}