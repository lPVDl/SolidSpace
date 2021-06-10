namespace SolidSpace.UI
{
    public interface IUIManager
    {
        public bool IsMouseOver { get; }
        
        T Instantiate<T>(UIPrefab<T> prefab) where T : class, IUIElement;
        void AttachToRoot(IUIElement view, string rootContainerName);
    }
}