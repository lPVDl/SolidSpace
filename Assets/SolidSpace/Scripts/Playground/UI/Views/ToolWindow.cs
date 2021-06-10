using System;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    internal class ToolWindow : IToolWindow
    {
        public VisualElement Root { get; set; }
        
        public VisualElement AttachPoint { get; set; }
        
        public Label Label { get; set; }
        
        public void AttachChild(IUIElement view)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            
            AttachPoint.Add(view.Root);
        }

        public void SetTitle(string text)
        {
            Label.text = text;
        }

        public void SetVisible(bool isVisible)
        {
            Root.visible = isVisible;
        }
    }
}