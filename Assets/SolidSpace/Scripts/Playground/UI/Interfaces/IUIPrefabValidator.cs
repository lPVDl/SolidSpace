using SolidSpace.DataValidation;

namespace SolidSpace.Playground.UI
{
    public interface IUIPrefabValidator<T> : IDataValidator<UIPrefab<T>> where T : IUIElement
    {
        
    }
}