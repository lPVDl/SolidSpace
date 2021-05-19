using System.Collections.Generic;
using SolidSpace.DataValidation;

namespace SolidSpace.GameCycle
{
    internal class ConfigValidator : IDataValidator<Config>
    {
        private readonly HashSet<EControllerType> _itemHash;

        public ConfigValidator()
        {
            _itemHash = new HashSet<EControllerType>();
        }
        
        public string Validate(Config data)
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