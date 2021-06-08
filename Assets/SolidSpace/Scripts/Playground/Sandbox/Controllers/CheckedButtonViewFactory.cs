using SolidSpace.Playground.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    public class CheckedButtonViewFactory : AUIFactory<ICheckedButtonView>
    {
        protected override ICheckedButtonView Create(VisualElement source)
        {
            var view = new CheckedButtonView
            {
                Source = source
            };
            
            source.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }
    }
}