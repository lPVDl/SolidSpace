using System;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    public interface IToolButtonView : IUIElement
    {
        public event Action OnClick;

        void SetChecked(bool isChecked);
    }
}