using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    internal interface IToolWindowView : IUIElement
    {
        void AttachChild(IUIElement view);
    }
}