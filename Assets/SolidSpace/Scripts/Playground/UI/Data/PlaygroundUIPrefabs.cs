using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.UI
{
    [System.Serializable]
    internal class PlaygroundUIPrefabs
    {
        public UIPrefab<ToolButton> ToolButton => _toolButton;
        public UIPrefab<ToolWindow> ToolWindow => _toolWindow;
        public UIPrefab<TagLabel> TagLabel => _tagLabel;
        public UIPrefab<LayoutGrid> LayoutGrid => _layoutGrid;
        public UIPrefab<GeneralButton> GeneralButton => _generalButton;
        
        [SerializeField] private UIPrefab<ToolButton> _toolButton;
        [SerializeField] private UIPrefab<ToolWindow> _toolWindow;
        [SerializeField] private UIPrefab<TagLabel> _tagLabel;
        [SerializeField] private UIPrefab<LayoutGrid> _layoutGrid;
        [SerializeField] private UIPrefab<GeneralButton> _generalButton;
    }
}