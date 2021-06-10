using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    internal class LayoutGrid : ILayoutGrid
    {
        public VisualElement Root { get; set; }

        public void AttachChild(IUIElement child)
        {
            Root.Add(child.Root);
        }

        public void SetFlexWrap(Wrap wrap)
        {
            Root.style.flexWrap = wrap;
        }

        public void SetFlexDirection(FlexDirection direction)
        {
            Root.style.flexDirection = direction;
        }

        public void SetAlignItems(Align align)
        {
            Root.style.alignItems = align;
        }

        public void SetJustify(Justify justify)
        {
            Root.style.justifyContent = justify;
        }
    }
}