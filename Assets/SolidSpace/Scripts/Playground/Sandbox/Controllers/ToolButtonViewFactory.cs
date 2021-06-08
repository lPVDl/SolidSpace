using SolidSpace.Playground.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    public class ToolButtonViewFactory : AUIFactory<IToolButtonView>
    {
        protected override IToolButtonView Create(VisualElement source)
        {
            var view = new ToolButtonView
            {
                Source = source
            };
            
            source.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }
    }
}