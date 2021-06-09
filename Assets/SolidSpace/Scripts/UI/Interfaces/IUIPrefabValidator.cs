using SolidSpace.DataValidation;

namespace SolidSpace.UI
{
    public interface IUIPrefabValidator<T> : IDataValidator<UIPrefab<T>> where T : IUIElement
    {
        
    }
}