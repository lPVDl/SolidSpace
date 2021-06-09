using UnityEngine.UIElements;
using System;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal class ToolButtonView : IToolButtonView
    {
        public event Action OnClick;
        public VisualElement Source { get; set; }
        public VisualElement Button { get; set; }
        public VisualElement Image { get; set; }

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
                Image.AddToClassList("selected");
            }
            else
            {
                Button.RemoveFromClassList("selected");
                Image.RemoveFromClassList("selected");
            }
        }

        public void SetIcon(Sprite icon)
        {
            Image.style.backgroundImage = new StyleBackground(icon);
        }
    }
}