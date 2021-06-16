using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    [InspectorDataValidator]
    public class StringFieldFactory : AUIFactory<StringField>, IDataValidator<UIPrefab<StringField>>, 
        IStringFieldCorrectionBehaviour
    {
        private readonly IUIEventManager _events;
        private readonly UITreeAssetValidator _assetValidator;
        
        private StringFieldFactory()
        {
            _assetValidator = new UITreeAssetValidator();
        }

        public StringFieldFactory(IUIEventManager events)
        {
            _events = events;
        }
        
        protected override StringField Create(VisualElement root)
        {
            var view = new StringField
            {
                Root = root,
                TextField = UIQuery.Child<TextField>(root, ""),
                IsValueChanged = false,
                CorrectionBehaviour = this
            };
            
            _events.Register<ChangeEvent<string>>(view.TextField, view.OnValueChanged);
            _events.Register<FocusOutEvent>(view.TextField, view.OnFocusOut);

            return view;
        }

        public string Validate(UIPrefab<StringField> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null";
            }
            
            _assetValidator.SetAsset(data.Asset);

            if (!_assetValidator.TreeHasChild<TextField>("", out var message))
            {
                return message;
            }

            return string.Empty;
        }

        public string TryFixString(string value, out bool wasFixed)
        {
            wasFixed = false;
            
            return default;
        }
    }
}