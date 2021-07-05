using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class StringFieldFactory : AUIViewFactory<StringField>, IDataValidator<UIPrefab<StringField>>, 
        IStringFieldCorrectionBehaviour
    {
        private readonly IUIEventDispatcher _eventDispatcher;
        private readonly UITreeAssetValidator _assetValidator;
        
        private StringFieldFactory()
        {
            _assetValidator = new UITreeAssetValidator();
        }

        public StringFieldFactory(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        
        protected override StringField Create(VisualElement root)
        {
            var view = new StringField
            {
                Root = root,
                TextField = UIQuery.Child<TextField>(root, ""),
                IsValueChanged = false,
                CorrectionBehaviour = this,
                EventDispatcher = _eventDispatcher
            };
            
            view.TextField.RegisterCallback<ChangeEvent<string>>(view.OnValueChanged);
            view.TextField.RegisterCallback<FocusOutEvent>(view.OnFocusOut);

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