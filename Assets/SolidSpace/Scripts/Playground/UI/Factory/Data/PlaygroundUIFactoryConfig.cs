using SolidSpace.Playground.UI.Elements;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.UI.Factory
{
    [System.Serializable]
    internal class PlaygroundUIFactoryConfig
    {
        public UIPrefab<ToolButton> ToolButtonPrefab => _toolButtonPrefab;
        public UIPrefab<ToolWindow> ToolWindowPrefab => _toolWindowPrefab;
        
        [SerializeField] private UIPrefab<ToolButton> _toolButtonPrefab;
        [SerializeField] private UIPrefab<ToolWindow> _toolWindowPrefab;
    }
}