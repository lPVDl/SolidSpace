using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    internal class ToolWindowFactory : AUIViewFactory<ToolWindow>, IDataValidator<UIPrefab<ToolWindow>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public ToolWindowFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        protected override ToolWindow Create(VisualElement root)
        {
            return new ToolWindow
            {
                Root = root,
                AttachPoint = UIQuery.Child<VisualElement>(root, "AttachPoint"),
                Label = UIQuery.Child<Label>(root, "Label")
            };
        }

        public string Validate(UIPrefab<ToolWindow> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null'";
            }
            
            _treeValidator.SetAsset(data.Asset);
            
            if (!_treeValidator.TreeHasChild<VisualElement>("AttachPoint", out var message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<Label>("Label", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}