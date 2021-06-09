using SolidSpace.Playground.Core.Views;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Core
{
    internal class ToolButtonViewFactory : AUIFactory<IToolButtonView>, IUIPrefabValidator<IToolButtonView>
    {
        protected override IToolButtonView Create(VisualElement source)
        {
            var view = new ToolButtonView
            {
                Source = source,
                Button = source.Query<VisualElement>("Button").First(),
                Image = source.Query<VisualElement>("Image").First()
            };
            
            source.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }

        public string Validate(UIPrefab<IToolButtonView> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null";
            }

            if (!UIAssetValidator.TreeHasChildElement<VisualElement>(data.Asset, "Button", out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}