using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    public interface IToolWindow : IUIElement
    {
        void AttachChild(IUIElement view);
        
        void SetTitle(string text);
    }
}