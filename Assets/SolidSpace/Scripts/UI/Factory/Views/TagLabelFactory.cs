using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    internal class TagLabelFactory : AUIViewFactory<TagLabel>, IDataValidator<UIPrefab<TagLabel>>
    {
        private readonly IUIEventManager _events;
        private readonly UITreeAssetValidator _treeValidator;

        private TagLabelFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public TagLabelFactory(IUIEventManager events)
        {
            _events = events;
        }

        protected override TagLabel Create(VisualElement root)
        {
            var view = new TagLabel
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Label = UIQuery.Child<Label>(root, "Label"),
                Lock = UIQuery.Child<VisualElement>(root, "Lock"),
                State = ETagLabelState.Neutral,
                IsLocked = false
            };
            
            view.AddToClassList(TagLabel.StateToName(ETagLabelState.Neutral));
            view.AddToClassList("unlocked");
            
            _events.Register<MouseDownEvent>(view.Button, view.OnMouseDownEvent);

            return view;
        }

        public string Validate(UIPrefab<TagLabel> data)
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
            
            if (!_treeValidator.TreeHasChild<Label>("Label", out message))
            {
                return message;
            }
            
            if (!_treeValidator.TreeHasChild<VisualElement>("Lock", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}