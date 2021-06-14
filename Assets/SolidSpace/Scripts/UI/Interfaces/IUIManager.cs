namespace SolidSpace.UI
{
    public interface IUIManager
    {
        public bool IsMouseOver { get; }
        
        T Instantiate<T>(UIPrefab<T> prefab) where T : class, IUIElement;
        void AddToRoot(IUIElement view, string rootContainerName);
        void RemoveFromRoot(IUIElement view, string rootContainerName);
    }
}