using System.Collections.Generic;

namespace SpaceSimulator.Runtime
{
    [System.Serializable]
    public class GameCycleConfig
    {
        [Serialize] public IReadOnlyList<EControllerType> InvocationOrder { get; private set; }
    }
}