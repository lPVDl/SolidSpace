using System;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal interface IToolButtonView : IUIElement
    {
        public event Action OnClick;

        void SetSelected(bool isSelected);
        void SetIcon(Sprite icon);
    }
}