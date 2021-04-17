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
        
        public void Validate(GameCycleConfig data, ValidationResult result)
        {
            var order = data.InvocationOrder;
            _itemHash.Clear();
            
            for (var i = 0; i < order.Count; i++)
            {
                var item = order[i];

                if (order[i] == EControllerType.Invalid)
                {
                    result.IsError = true;
                    result.Message = $"{nameof(data.InvocationOrder)} contains '{EControllerType.Invalid}'";
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