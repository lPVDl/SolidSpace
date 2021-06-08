using System.Reflection.Emit;

namespace SolidSpace.Playground.UI
{
    public interface IUIManager
    {
        public bool IsMouseOver { get; }
        
        T Instantiate<T>(UIPrefab<T> prefab) where T : IUIElement;
        void AttachToRoot(IUIElement view, string rootContainerName);
    }
}