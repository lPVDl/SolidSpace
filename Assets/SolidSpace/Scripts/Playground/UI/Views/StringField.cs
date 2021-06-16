using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    public class StringField : IStringField
    {
        public VisualElement Root { get; set; }
        
        public TextField TextField { get; set; }
        
        public void SetLabel(string text)
        {
            TextField.label = text;
        }

        public void SetValue(string value)
        {
            TextField.SetValueWithoutNotify(value);
        }
    }
}