using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimulator
{
    [System.Serializable]
    public class GameCycleConfig
    {
        public IReadOnlyList<EControllerType> InvocationOrder => _invocationOrder;

        [SerializeField] private List<EControllerType> _invocationOrder;
    }
}