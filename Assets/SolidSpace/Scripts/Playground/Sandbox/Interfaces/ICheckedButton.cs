using System;

namespace SolidSpace.Playground.UI
{
    public interface ICheckedButton : IUIElement
    {
        public event Action OnClick;

        void SetChecked(bool isActive);
    }
}