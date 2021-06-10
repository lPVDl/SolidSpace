using System;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI.Elements
{
    internal class ToolWindow : IToolWindow
    {
        public VisualElement Source { get; set; }
        
        public VisualElement AttachPoint { get; set; }
        
        public Label Label { get; set; }
        
        public void AttachChild(IUIElement view)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            
            AttachPoint.Add(view.Source);
        }

        public void SetTitle(string text)
        {
            Label.text = text;
        }
    }
}