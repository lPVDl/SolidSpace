using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class GeneralButtonFactory : AUIViewFactory<GeneralButton>, IDataValidator<UIPrefab<GeneralButton>>
    {
        private readonly IUIEventManager _events;
        private readonly UITreeAssetValidator _treeValidator;
        
        private GeneralButtonFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }

        public GeneralButtonFactory(IUIEventManager events)
        {
            _events = events;
        }
        
        protected override GeneralButton Create(VisualElement root)
        {
            var view = new GeneralButton
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Label = UIQuery.Child<Label>(root, "Label"),
                IsMouseDown = false,
            };
            
            _events.Register<MouseDownEvent>(view.Button, view.OnMouseDown);
            _events.Register<MouseUpEvent>(view.Button, view.OnMouseUp);
            _events.Register<MouseLeaveEvent>(view.Button, view.OnMouseLeave);

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