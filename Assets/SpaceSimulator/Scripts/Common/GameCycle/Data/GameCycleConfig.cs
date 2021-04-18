using System.Collections.Generic;
using SpaceSimulator.Enums;
using UnityEngine;

namespace SpaceSimulator.Data
{
    [System.Serializable]
    public class GameCycleConfig
    {
        public IReadOnlyList<EControllerType> InvocationOrder => _invocationOrder;

        [SerializeField] private List<EControllerType> _invocationOrder;
    }
}