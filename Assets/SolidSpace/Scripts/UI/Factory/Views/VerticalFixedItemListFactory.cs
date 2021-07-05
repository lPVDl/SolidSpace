using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class VerticalFixedItemListFactory : AUIViewFactory<VerticalFixedItemList>, IDataValidator<UIPrefab<VerticalFixedItemList>>
    {
        private readonly IUIEventManager _events;
        private readonly UITreeAssetValidator _treeValidator;
        
        private VerticalFixedItemListFactory()
        {
            _treeValidator = new UITreeAssetValidator();
        }

        public VerticalFixedItemListFactory(IUIEventManager events)
        {
            _events = events;
        }
        
        protected override VerticalFixedItemList Create(VisualElement root)
        {
            var view = new VerticalFixedItemList
            {
                Root = root,
                AttachPoint = UIQuery.Child<VisualElement>(root, "AttachPoint"),
                SliderStart = UIQuery.Child<VisualElement>(root, "SliderStart"),
                SliderMiddle = UIQuery.Child<VisualElement>(root, "SliderMiddle"),
                SliderEnd = UIQuery.Child<VisualElement>(root, "SliderEnd")
            };
            
            _events.Register<WheelEvent>(view.Root, view.OnWheelEvent);

            return view;
        }

        public string Validate(UIPrefab<VerticalFixedItemList> data)
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

            if (!_treeValidator.TreeHasChild<VisualElement>("SliderStart", out message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<VisualElement>("SliderMiddle", out message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<VisualElement>("SliderEnd", out message))
            {
                return message;
            }
            
            return string.Empty;
        }
    }
}