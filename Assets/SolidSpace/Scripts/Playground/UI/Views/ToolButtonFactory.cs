using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    [InspectorDataValidator]
    internal class ToolButtonFactory : AUIFactory<ToolButton>, IDataValidator<UIPrefab<ToolButton>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public ToolButtonFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        protected override ToolButton Create(VisualElement root)
        {
            var view = new ToolButton
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Image = UIQuery.Child<VisualElement>(root, "Image"),
            };
            
            root.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

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