using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    [System.Serializable]
    internal class Config
    {
        public IReadOnlyList<EControllerType> InvocationOrder => _invocationOrder;

        [SerializeField] private List<EControllerType> _invocationOrder;
    }
}