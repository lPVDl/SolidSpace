using System;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    public class ToolButtonView : IToolButtonView
    {
        public event Action OnClick;
        public VisualElement Source { get; set; }

        public void OnMouseDownEvent(MouseDownEvent evt)
        {
            OnClick?.Invoke();
        }

        public void SetChecked(bool isActive)
        {
            Source.SetEnabled(isActive);   
        }
    }
}