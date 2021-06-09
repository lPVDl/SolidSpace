using SolidSpace.UI;

namespace SolidSpace.Playground.Core
{
    internal interface IToolWindowView : IUIElement
    {
        void AttachChild(IUIElement view);
    }
}