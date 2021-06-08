using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    [System.Serializable]
    public class UIPrefab<T> where T : IUIElement
    {
        public VisualTreeAsset Asset => _asset;
        
        [SerializeField] private VisualTreeAsset _asset;
    }
}