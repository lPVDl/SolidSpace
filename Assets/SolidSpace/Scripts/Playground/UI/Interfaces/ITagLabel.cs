using System;

namespace SolidSpace.Playground.UI
{
    public interface ITagLabel
    {
        event Action Clicked;

        void SetState(ETagLabelState state);

        void SetLabel(string label);
    }
}