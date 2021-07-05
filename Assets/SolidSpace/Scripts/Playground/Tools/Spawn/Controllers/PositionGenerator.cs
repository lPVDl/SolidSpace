using System.Collections.Generic;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Spawn
{
    internal class PositionGenerator
    {
        private float2[] _positions;
        private int _radius;
        private int _amount;
        
        public PositionGenerator()
        {
            _positions = new float2[0];
        }

        public IEnumerable<float2> IteratePositions(float2 center, int radius, int amount)
        {
            if ((_amount != amount) || (radius != _radius))
            {
                _radius = radius;
                _amount = amount;
                
                GeneratePositions(radius, amount);
            }

            for (var i = 0; i < amount; i++)
            {
                yield return center + _positions[i];
            }
        }

        private void GeneratePositions(float radius, int amount)
        {
            if (_positions.Length < amount)
            {
                _positions = new float2[amount];
            }

            for (var i = 0; i < amount; i++)
            {
                var pos = UnityEngine.Random.insideUnitCircle * radius;
                _positions[i] = new float2(pos.x, pos.y);
            }
        }
    }
}