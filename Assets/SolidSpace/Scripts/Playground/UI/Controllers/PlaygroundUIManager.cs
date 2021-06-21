using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    public class PlaygroundUIManager : IPlaygroundUIManager
    {
        private readonly IUIManager _uiManager;

        public PlaygroundUIManager(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }
        
        public void SetElementVisible(IUIElement uiElement, bool isVisible)
        {
            if (isVisible)
            {
                _uiManager.AttachToRoot(uiElement, "ContainerA");
            }
            else
            {
                _uiManager.DetachFromRoot(uiElement, "ContainerA");
            }
        }
    }
}