using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    internal class ToolButtonFactory : AUIViewFactory<ToolButton>, IDataValidator<UIPrefab<ToolButton>>
    {
        private readonly IUIEventManager _events;
        private readonly UITreeAssetValidator _treeValidator;
        
        private ToolButtonFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public ToolButtonFactory(IUIEventManager events)
        {
            _events = events;
        }

        protected override ToolButton Create(VisualElement root)
        {
            var view = new ToolButton
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Image = UIQuery.Child<VisualElement>(root, "Image"),
            };

            _events.Register<MouseDownEvent>(view.Button, view.OnMouseDownEvent);

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