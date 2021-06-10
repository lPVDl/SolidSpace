using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI.Elements
{
    [InspectorDataValidator]
    internal class ToolWindowFactory : AUIFactory<ToolWindow>, IDataValidator<UIPrefab<ToolWindow>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public ToolWindowFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        protected override ToolWindow Create(VisualElement source)
        {
            return new ToolWindow
            {
                Source = source,
                AttachPoint = UIQuery.Child<VisualElement>(source, "AttachPoint")
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

            return string.Empty;
        }
    }
}