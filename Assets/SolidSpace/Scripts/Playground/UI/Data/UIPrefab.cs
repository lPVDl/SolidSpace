using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    [System.Serializable]
    public class UIPrefab
    {
        internal VisualTreeAsset Asset => _asset;
        
        [SerializeField] private VisualTreeAsset _asset;
    }
}