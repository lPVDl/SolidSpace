using System.Collections.Generic;

namespace SpaceSimulator.Runtime
{
    public class GameCycleConfigValidator : IValidator<GameCycleConfig>
    {
        private readonly HashSet<EControllerType> _itemHash;

        public GameCycleConfigValidator()
        {
            _itemHash = new HashSet<EControllerType>();
        }
        
        public string Validate(GameCycleConfig data)
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