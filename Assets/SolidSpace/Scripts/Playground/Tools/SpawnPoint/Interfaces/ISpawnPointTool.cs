using System.Collections.Generic;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    public interface ISpawnPointTool
    {
        IEnumerable<float2> Update();
        void SetEnabled(bool isEnabled);
    }
}