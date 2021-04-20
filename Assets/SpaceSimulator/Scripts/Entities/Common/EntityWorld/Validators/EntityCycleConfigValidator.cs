using System.Collections.Generic;

namespace SpaceSimulator.Entities
{
    public class EntityCycleConfigValidator : IValidator<EntityCycleConfig>
    {
        private readonly HashSet<ESystemType> _itemHash;

        public EntityCycleConfigValidator()
        {
            _itemHash = new HashSet<ESystemType>();
        }
        
        public string Validate(EntityCycleConfig data)
        {
            var order = data.InvocationOrder;
            _itemHash.Clear();
            
            for (var i = 0; i < order.Count; i++)
            {
                var item = order[i];

                if (!_itemHash.Add(item))
                {
                    return $"'{item}' is duplicated in {nameof(data.InvocationOrder)}";
                }
            }

            return string.Empty;
        }
    }
}