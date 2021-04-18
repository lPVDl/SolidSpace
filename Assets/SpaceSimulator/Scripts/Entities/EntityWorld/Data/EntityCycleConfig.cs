using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimulator.Entities.EntityWorld
{
    [System.Serializable]
    public class EntityCycleConfig
    {
        public IReadOnlyList<ESystemType> InvocationOrder => _invocationOrder;

        [SerializeField] private List<ESystemType> _invocationOrder;
    }
}