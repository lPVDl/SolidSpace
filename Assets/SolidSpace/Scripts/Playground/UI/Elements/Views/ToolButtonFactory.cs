using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI.Elements
{
    [InspectorDataValidator]
    internal class ToolButtonFactory : AUIFactory<ToolButton>, IDataValidator<UIPrefab<ToolButton>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public ToolButtonFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        protected override ToolButton Create(VisualElement source)
        {
            var view = new ToolButton
            {
                Source = source,
                Button = UIQuery.Child<VisualElement>(source, "Button"),
                Image = UIQuery.Child<VisualElement>(source, "Image"),
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
            
            _treeValidator.SetAsset(data.Asset);

            if (!_treeValidator.TreeHasChild<VisualElement>("Button", out var message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<VisualElement>("Image", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}