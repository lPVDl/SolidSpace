using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    public class MinMaxIntRangeBehaviour : IStringFieldCorrectionBehaviour
    {
        private readonly int _min;
        private readonly int _max;
        
        public MinMaxIntRangeBehaviour(int min, int max)
        {
            _min = min;
            _max = max;
        }
        
        public string TryFixString(string value, out bool wasFixed)
        {
            wasFixed = true;

            if (!int.TryParse(value, out var parsedInt))
            {
                return _min.ToString();
            }

            if (parsedInt < _min)
            {
                return _min.ToString();
            }

            if (parsedInt > _max)
            {
                return _max.ToString();
            }

            wasFixed = false;
            
            return default;
        }
    }
}