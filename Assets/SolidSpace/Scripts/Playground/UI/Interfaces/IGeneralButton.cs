using System;
using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    public interface IGeneralButton : IUIElement
    {
        public event Action Clicked;

        void SetLabel(string text);
    }
}