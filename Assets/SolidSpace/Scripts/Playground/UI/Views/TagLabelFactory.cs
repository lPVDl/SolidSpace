using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    [InspectorDataValidator]
    internal class TagLabelFactory : AUIFactory<TagLabel>, IDataValidator<UIPrefab<TagLabel>>
    {
        private readonly UITreeAssetValidator _treeValidator;

        public TagLabelFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        protected override TagLabel Create(VisualElement root)
        {
            var view = new TagLabel
            {
                Root = root,
                Label = UIQuery.Child<Label>(root, "Label")
            };
            
            root.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }

        public string Validate(UIPrefab<TagLabel> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null";
            }
            
            _treeValidator.SetAsset(data.Asset);

            if (!_treeValidator.TreeHasChild<Label>("Label", out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}