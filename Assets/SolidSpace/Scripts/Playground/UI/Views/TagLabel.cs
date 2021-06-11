using System;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    internal class TagLabel : ITagLabel
    {
        public event Action Clicked;
        
        public VisualElement Root { get; set; }
        public VisualElement Button { get; set; }
        public Label Label { get; set; }
        public VisualElement Lock { get; set; }
        public ETagLabelState State { get; set; }

        public void SetState(ETagLabelState newState)
        {
            if (State == newState)
            {
                return;
            }

            RemoveFromClassList(StateToName(State));

            State = newState;

            AddToClassList(StateToName(State));
        }

        public static string StateToName(ETagLabelState state)
        {
            switch (state)
            {
                case ETagLabelState.Positive: return "positive";
                case ETagLabelState.Negative: return "negative";
                case ETagLabelState.Neutral: return "neutral";

                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void RemoveFromClassList(string className)
        {
            Button.RemoveFromClassList(className);
            Label.RemoveFromClassList(className);
            Lock.RemoveFromClassList(className);
        }

        public void AddToClassList(string className)
        {
            Button.AddToClassList(className);
            Label.AddToClassList(className);
            Lock.AddToClassList(className);
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