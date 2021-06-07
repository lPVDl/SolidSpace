namespace SolidSpace.Playground.UI
{
    public interface IUIManager
    {
        UIElementHandle CreateElement(UIPrefab prefab);
        void AttachElementToRoot(UIElementHandle element, string rootContainerName);
    }
}