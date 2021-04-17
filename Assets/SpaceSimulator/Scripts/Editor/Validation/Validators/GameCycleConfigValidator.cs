using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Validation;
using SpaceSimulator.Runtime;

namespace SpaceSimulator.Editor.Validation
{
    public class GameCycleConfigValidator : ValueValidator<GameCycleConfig>
    {
        private static readonly HashSet<EControllerType> ItemHash = new HashSet<EControllerType>();
        
        protected override void Validate(ValidationResult result)
        {
            var invocationOrder = ValueEntry.SmartValue.InvocationOrder;
            ItemHash.Clear();
            for (var i = 0; i < invocationOrder.Count; i++)
            {
                var item = invocationOrder[i];
                
                if (invocationOrder[i] == EControllerType.Invalid)
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = $"{nameof(ValueEntry.SmartValue.InvocationOrder)} can not contain 'Invalid'";
                    return;
                }

                if (!ItemHash.Add(item))
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = $"Element '{item}' is duplicated in {nameof(ValueEntry.SmartValue.InvocationOrder)}";
                }
            }
        }
    }
}