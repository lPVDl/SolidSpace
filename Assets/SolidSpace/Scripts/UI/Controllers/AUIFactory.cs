using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    public abstract class AUIFactory<T> : IUIFactory where T : class, IUIElement
    {
        public Type ViewType => typeof(T);

        object IUIFactory.Create(VisualElement root)
        {
            return Create(root);
        }

        protected abstract T Create(VisualElement root);
    }
}