namespace SolidSpace.Playground.UI
{
    public interface IUIManager
    {
        T Instantiate<T>(UIPrefab<T> prefab) where T : IUIElement;
        void AttachToRoot(IUIElement view, string rootContainerName);
    }
}