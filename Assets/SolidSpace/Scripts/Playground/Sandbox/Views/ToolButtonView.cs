using UnityEngine.UIElements;
using System;

namespace SolidSpace.Playground.Sandbox
{
    public class ToolButtonView : IToolButtonView
    {
        public event Action OnClick;
        public VisualElement Source { get; set; }
        public VisualElement Button { get; set; }

        private bool _isChecked;
        
        public void OnMouseDownEvent(MouseDownEvent evt)
        {
            OnClick?.Invoke();

            SetChecked(!_isChecked);
        }

        public void SetChecked(bool isChecked)
        {
            if (isChecked == _isChecked)
            {
                return;
            }

            _isChecked = isChecked;
            
            if (_isChecked)
            {
                Button.AddToClassList("selected");
            }
            else
            {
                Button.RemoveFromClassList("selected");
            }
        }
    }
}