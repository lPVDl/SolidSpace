using UnityEngine.UIElements;
using System;

namespace SolidSpace.Playground.Sandbox
{
    public class ToolButtonView : IToolButtonView
    {
        public event Action OnClick;
        public VisualElement Source { get; set; }
        public VisualElement Button { get; set; }

        private bool _isSelected;
        
        public void OnMouseDownEvent(MouseDownEvent evt)
        {
            OnClick?.Invoke();
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected == _isSelected)
            {
                return;
            }

            _isSelected = isSelected;
            
            if (_isSelected)
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