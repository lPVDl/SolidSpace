using SolidSpace.Playground.UI.Elements;

namespace SolidSpace.Playground.UI.Factory
{
    public interface IPlaygroundUIFactory
    {
        IToolWindow CreateToolWindow();

        IToolButton CreateToolButton();
    }
}