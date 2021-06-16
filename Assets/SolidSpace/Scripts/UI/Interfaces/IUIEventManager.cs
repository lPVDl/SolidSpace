using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    public interface IUIEventManager
    {
        void Register<T>(VisualElement element, Action<T> handler) where T : EventBase<T>, new();

        void Unregister<T>(VisualElement element, Action<T> handler) where T : EventBase<T>, new();
    }
}