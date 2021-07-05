using System;
using SolidSpace.UI.Core;

namespace SolidSpace.UI.Factory.Intefaces
{
    public interface IGeneralButton : IUIElement
    {
        public event Action Clicked;

        void SetLabel(string text);
    }
}