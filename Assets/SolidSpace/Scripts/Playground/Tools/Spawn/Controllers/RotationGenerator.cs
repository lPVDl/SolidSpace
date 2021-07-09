using System.Collections;
using System.Collections.Generic;
using SolidSpace.Mathematics;

namespace SolidSpace.Playground.Tools.Spawn
{
    public class RotationGenerator
    {
        private float[] _rotations;
        private int _seed;
        private int _amount;

        public RotationGenerator()
        {
            _rotations = new float[0];
        }

        public IReadOnlyList<float> IterateRotations(int seed, int amount)
        {
            if ((_amount != amount) || (_seed != seed))
            {
                _amount = amount;
                _seed = seed;

                GenerateRotations(_seed, _amount);
            }

            return _rotations;
        }

        private void GenerateRotations(int seed, int amount)
        {
            if (_rotations.Length < amount)
            {
                _rotations = new float[amount];
            }

            UnityEngine.Random.InitState(seed);
            
            for (var i = 0; i < amount; i++)
            {
                _rotations[i] = UnityEngine.Random.value * FloatMath.TwoPI;
            }
        }
    }
}