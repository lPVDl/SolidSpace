using SolidSpace.UI.Core;

namespace SolidSpace.UI.Factory.Intefaces
{
    public interface IToolWindow : IUIElement
    {
        void AttachChild(IUIElement view);
        
        void SetTitle(string text);
    }
}