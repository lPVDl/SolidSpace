using System;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Core.Views
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