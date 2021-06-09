using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    public abstract class AUIFactory<T> : IUIFactory
    {
        public Type ViewType => typeof(T);
        
        object IUIFactory.Create(VisualElement source)
        {
            return Create(source);
        }

        protected abstract T Create(VisualElement source);
    }
}