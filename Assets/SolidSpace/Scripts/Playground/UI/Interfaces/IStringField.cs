using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    public interface IStringField : IUIElement
    {
        void SetLabel(string text);

        void SetValue(string value);
    }
}