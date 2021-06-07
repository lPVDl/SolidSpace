using System;

namespace SolidSpace.Playground.UI
{
    public interface IUIFactory
    {
        public Type OutputElementType { get; }
        
        object CreateElement(UIElementHandle handle);
    }
}