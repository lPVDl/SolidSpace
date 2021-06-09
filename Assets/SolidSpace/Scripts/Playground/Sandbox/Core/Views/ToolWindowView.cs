using System;
using SolidSpace.Playground.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    internal class ToolWindowView : IToolWindowView
    {
        public VisualElement Source { get; set; }
        
        public VisualElement AttachPoint { get; set; }
        
        public void AttachChild(IUIElement view)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            
            AttachPoint.Add(view.Source);
        }
    }
}