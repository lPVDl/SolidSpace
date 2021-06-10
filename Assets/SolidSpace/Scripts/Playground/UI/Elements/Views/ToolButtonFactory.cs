using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI.Elements
{
    internal class ToolButtonFactory : AUIFactory<IToolButton>, IUIPrefabValidator<ToolButton>
    {
        protected override IToolButton Create(VisualElement source)
        {
            var view = new ToolButton
            {
                Source = source,
                Button = source.Query<VisualElement>("Button").First(),
                Image = source.Query<VisualElement>("Image").First()
            };
            
            source.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }

        public string Validate(UIPrefab<ToolButton> data)
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