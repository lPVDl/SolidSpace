using System.Collections.Generic;

namespace SpaceSimulator.Entities.EntityWorld
{
    [System.Serializable]
    public class EntityCycleConfig
    {
        [Serialize] public IReadOnlyList<ESystemType> InvocationOrder { get; private set; }
    }
}