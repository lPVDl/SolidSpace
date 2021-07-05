using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    public abstract class AUIViewFactory<T> : IUIViewFactory where T : class, IUIElement
    {
        public Type ViewType => typeof(T);

        object IUIViewFactory.Create(VisualElement root)
        {
            return Create(root);
        }

        protected abstract T Create(VisualElement root);
    }
}