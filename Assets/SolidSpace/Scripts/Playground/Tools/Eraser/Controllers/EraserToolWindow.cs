using SolidSpace.Playground.UI;
using SolidSpace.UI;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserToolWindow
    {
        private readonly IToolWindow _window;
        
        public EraserToolWindow(IPlaygroundUIFactory uiFactory, IUIManager uiManager)
        {
            _window = uiFactory.CreateToolWindow();
            _window.SetTitle("Filter");
            
            uiManager.AttachToRoot(_window, "ContainerA");
        }

        public void SetVisible(bool isVisible)
        {
            _window.SetVisible(isVisible);
        }
    }
}