using System;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    internal interface IToolButtonView : IUIElement
    {
        public event Action OnClick;

        void SetSelected(bool isSelected);
        void SetIcon(Sprite icon);
    }
}