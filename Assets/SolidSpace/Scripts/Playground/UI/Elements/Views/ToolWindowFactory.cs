using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI.Elements
{
    internal class ToolWindowFactory : AUIFactory<IToolWindow>, IUIPrefabValidator<ToolWindow>
    {
        protected override IToolWindow Create(VisualElement source)
        {
            return new ToolWindow
            {
                Source = source,
                AttachPoint = source.Query<VisualElement>("AttachPoint").First()
            };
        }

        public string Validate(UIPrefab<ToolWindow> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null'";
            }
            
            if (!UIAssetValidator.TreeHasChildElement<VisualElement>(data.Asset, "AttachPoint", out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}