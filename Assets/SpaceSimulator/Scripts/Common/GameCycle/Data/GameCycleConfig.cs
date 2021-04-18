using System.Collections.Generic;
using SpaceSimulator.Enums;

namespace SpaceSimulator.Data
{
    [System.Serializable]
    public class GameCycleConfig
    {
        [Serialize] public IReadOnlyList<EControllerType> InvocationOrder { get; private set; }
    }
}