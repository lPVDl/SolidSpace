using System;
using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    public interface ITagLabel : IUIElement
    {
        event Action Clicked;

        void SetState(ETagLabelState state);

        void SetLabel(string label);
    }
}