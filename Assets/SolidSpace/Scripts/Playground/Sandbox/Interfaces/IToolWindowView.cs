using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    public interface IToolWindowView : IUIElement
    {
        void AttachChild(IUIElement view);
    }
}