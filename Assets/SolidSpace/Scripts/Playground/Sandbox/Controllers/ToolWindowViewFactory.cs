using SolidSpace.Playground.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    public class ToolWindowViewFactory : AUIFactory<IToolWindowView>, IUIPrefabValidator<IToolWindowView>
    {
        private const string AttachPoint = "AttachPoint";

        protected override IToolWindowView Create(VisualElement source)
        {
            return new ToolWindowView
            {
                Source = source,
                AttachPoint = source.Query<VisualElement>(AttachPoint).First()
            };
        }

        public string Validate(UIPrefab<IToolWindowView> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)} is null'";
            }
            
            if (!UIAssetValidator.TreeHasChildElement<VisualElement>(data.Asset, AttachPoint, out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}