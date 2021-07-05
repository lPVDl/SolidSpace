using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    public interface IUIViewFactory
    {
        public Type ViewType { get; }

        object Create(VisualElement source);
    }
}