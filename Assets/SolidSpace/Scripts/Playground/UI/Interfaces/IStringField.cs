using System;
using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    public interface IStringField : IUIElement
    {
        event Action ValueChanged;
        
        public string Value { get; }
        
        void SetLabel(string text);
        void SetValue(string value);
        
        void SetValueCorrectionBehaviour(IStringFieldCorrectionBehaviour behaviour);
    }
}