using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using SpaceSimulator.Runtime;

namespace SpaceSimulator.Editor.Validation
{
    public class NullReferenceValidator : AttributeValidator<SerializeAttribute>
    {
        protected override void Validate(ValidationResult result)
        {
            if (Property.BaseValueEntry.ValueState == PropertyValueState.NullReference)
            {
                result.ResultType = ValidationResultType.Error;

                if (int.TryParse(Property.NiceName, out _))
                {
                    result.Message = $"Element at index ({Property.NiceName}) is null";
                    return;
                }

                result.Message = $"Property '{Property.NiceName}' is null";
            }
        }
    }
}