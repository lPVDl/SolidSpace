namespace SolidSpace.Playground.UI
{
    public interface IPlaygroundUIFactory
    {
        IToolWindow CreateToolWindow();
        IToolButton CreateToolButton();
        ITagLabel CreateFilterLabel();
    }
}