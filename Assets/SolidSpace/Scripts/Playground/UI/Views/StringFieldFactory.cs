using SolidSpace.DataValidation;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    [InspectorDataValidator]
    public class StringFieldFactory : AUIFactory<StringField>, IDataValidator<UIPrefab<StringField>>
    {
        private readonly UITreeAssetValidator _assetValidator;
        
        public StringFieldFactory()
        {
            _assetValidator = new UITreeAssetValidator();
        }
        
        protected override StringField Create(VisualElement root)
        {
            return new StringField
            {
                Root = root,
                TextField = UIQuery.Child<TextField>(root, "")
            };
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
    }
}