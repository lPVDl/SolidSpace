using SolidSpace.UI;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public interface IComponentFilterFactory
    {
        IComponentFilter Create(params ComponentType[] readonlyEnabledComponents);
        IUIElement CreateReadonly(params ComponentType[] components);
    }
}