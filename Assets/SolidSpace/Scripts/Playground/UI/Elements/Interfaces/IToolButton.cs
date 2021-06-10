using System;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.UI.Elements
{
    public interface IToolButton : IUIElement
    {
        public event Action OnClick;

        void SetSelected(bool isSelected);
        void SetIcon(Sprite icon);
    }
}