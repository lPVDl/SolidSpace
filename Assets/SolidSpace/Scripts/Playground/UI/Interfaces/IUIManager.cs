namespace SolidSpace.Playground.UI
{
    public interface IUIManager
    {
        // UIElementHandle CreateElement(UIPrefab prefab);
        // void AttachElementToRoot(UIElementHandle element, string rootContainerName);
        // UIElementHandle QueryElement(UIElementHandle handle, string name);
        // T CreateElement<T>(UIPrefab prefab) where T : IUIElementView, new();
        T Instantiate<T>(UIPrefab<T> prefab) where T : IUIElement;
        void AttachToRoot(IUIElement view, string rootContainerName);
    }
}