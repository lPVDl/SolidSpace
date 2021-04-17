using System.Collections.Generic;

namespace SpaceSimulator.Runtime.Entities
{
    [System.Serializable]
    public class EntityCycleConfig
    {
        [Serialize] public IReadOnlyList<ESystemType> InvocationOrder { get; private set; }
    }
}