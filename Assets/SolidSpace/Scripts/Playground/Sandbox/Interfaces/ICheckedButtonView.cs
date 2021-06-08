using System;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    public interface ICheckedButtonView : IUIElement
    {
        public event Action OnClick;

        void SetChecked(bool isActive);
    }
}