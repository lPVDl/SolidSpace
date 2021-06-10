using System;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    internal class TagLabel : ITagLabel
    {
        public event Action Clicked;
        
        public VisualElement Root { get; set; }
        public Label Label { get; set; }

        private ETagLabelState _state;
        
        public void SetState(ETagLabelState state)
        {
            if (_state == state)
            {
                return;
            }

            switch (_state)
            {
                case ETagLabelState.Positive:
                    Label.RemoveFromClassList("positive");
                    break;
                
                case ETagLabelState.Negative:
                    Label.RemoveFromClassList("negative");
                    break;
            }

            _state = state;

            switch (_state)
            {
                case ETagLabelState.Positive:
                    Label.AddToClassList("positive");
                    break;
                
                case ETagLabelState.Negative:
                    Label.AddToClassList("negative");
                    break;
            }
        }

        public void SetLabel(string label)
        {
            Label.text = label;
        }
        
        public void OnMouseDownEvent(MouseDownEvent e)
        {
            e.StopPropagation();
            
            Clicked?.Invoke();
        }
    }
}