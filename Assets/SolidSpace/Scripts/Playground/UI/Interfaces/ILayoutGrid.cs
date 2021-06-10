using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    public interface ILayoutGrid : IUIElement
    {
        void AttachChild(IUIElement child);
        void SetFlexWrap(Wrap wrap);
        void SetFlexDirection(FlexDirection direction);
        void SetAlignItems(Align align);
        void SetJustifyContent(Justify justify);
    }
}