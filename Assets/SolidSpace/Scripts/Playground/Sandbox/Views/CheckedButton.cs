using System;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    public class CheckedButton : ICheckedButton
    {
        public event Action OnClick;
        public UIElementHandle Handle { get; }

        public CheckedButton(UIElementHandle handle)
        {
            Handle = handle;
            
            Handle.element.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            OnClick?.Invoke();
        }

        public void SetChecked(bool isActive)
        {
            Handle.element.SetEnabled(isActive);   
        }
    }
}