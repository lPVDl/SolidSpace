namespace SolidSpace.Playground.UI
{
    public interface IPlaygroundUIFactory
    {
        IToolWindow CreateToolWindow();
        IToolButton CreateToolButton();
        ITagLabel CreateTagLabel();
        ILayoutGrid CreateLayoutGrid();
        IGeneralButton CreateGeneralButton();
    }
}