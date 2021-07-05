namespace SolidSpace.UI.Factory.Intefaces
{
    public interface IUIFactory
    {
        IToolWindow CreateToolWindow();
        IToolButton CreateToolButton();
        ITagLabel CreateTagLabel();
        ILayoutGrid CreateLayoutGrid();
        IGeneralButton CreateGeneralButton();
        IStringField CreateStringField();
    }
}