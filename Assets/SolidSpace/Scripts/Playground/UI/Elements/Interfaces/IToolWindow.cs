using SolidSpace.UI;

namespace SolidSpace.Playground.UI.Elements
{
    public interface IToolWindow : IUIElement
    {
        void AttachChild(IUIElement view);
    }
}