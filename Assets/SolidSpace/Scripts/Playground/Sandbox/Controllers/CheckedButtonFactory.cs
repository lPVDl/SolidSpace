using System;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    public class CheckedButtonFactory : IUIFactory
    {
        public Type OutputElementType => typeof(ICheckedButton);
        
        public object CreateElement(UIElementHandle handle)
        {
            return new CheckedButton(handle);
        }
    }
}