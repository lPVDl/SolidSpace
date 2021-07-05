using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class GeneralButtonFactory : AUIViewFactory<GeneralButton>, IDataValidator<UIPrefab<GeneralButton>>
    {
        private readonly IUIEventDispatcher _eventDispatcher;
        private readonly UITreeAssetValidator _treeValidator;
        
        private GeneralButtonFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }

        public GeneralButtonFactory(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        
        protected override GeneralButton Create(VisualElement root)
        {
            var view = new GeneralButton
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Label = UIQuery.Child<Label>(root, "Label"),
                IsMouseDown = false,
                EventDispatcher = _eventDispatcher
            };
            
            view.Button.RegisterCallback<MouseDownEvent>(view.OnMouseDown);
            view.Button.RegisterCallback<MouseUpEvent>(view.OnMouseUp);
            view.Button.RegisterCallback<MouseLeaveEvent>(view.OnMouseLeave);

            return view;
        }

        public string Validate(UIPrefab<GeneralButton> data)
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

            return string.Empty;
        }
    }
}