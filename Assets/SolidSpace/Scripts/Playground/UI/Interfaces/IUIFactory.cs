using System;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    public interface IUIFactory
    {
        public Type ViewType { get; }

        object Create(VisualElement source);
    }
}