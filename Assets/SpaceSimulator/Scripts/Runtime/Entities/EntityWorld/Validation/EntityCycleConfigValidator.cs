using System.Collections.Generic;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityCycleConfigValidator : IValidator<EntityCycleConfig>
    {
        private readonly HashSet<ESystemType> _itemHash;

        public EntityCycleConfigValidator()
        {
            _itemHash = new HashSet<ESystemType>();
        }
        
        public void Validate(EntityCycleConfig data, ValidationResult result)
        {
            var order = data.InvocationOrder;
            _itemHash.Clear();
            
            for (var i = 0; i < order.Count; i++)
            {
                var item = order[i];

                if (order[i] == ESystemType.Invalid)
                {
                    result.IsError = true;
                    result.Message = $"{nameof(data.InvocationOrder)} contains '{ESystemType.Invalid}'";
                    return;
                }

                if (!_itemHash.Add(item))
                {
                    result.IsError = true;
                    result.Message = $"'{item}' is duplicated in {nameof(data.InvocationOrder)}";
                    return;
                }
            }
        }
    }
}