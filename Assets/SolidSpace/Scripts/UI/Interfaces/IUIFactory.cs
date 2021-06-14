using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    public interface IUIFactory
    {
        public Type ViewType { get; }

        object Create(VisualElement source);
    }
}