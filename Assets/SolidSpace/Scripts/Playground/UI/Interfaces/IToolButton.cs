using System;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.UI
{
    public interface IToolButton : IUIElement
    {
        public event Action Clicked;

        void SetSelected(bool isSelected);
        void SetIcon(Sprite icon);
    }
}