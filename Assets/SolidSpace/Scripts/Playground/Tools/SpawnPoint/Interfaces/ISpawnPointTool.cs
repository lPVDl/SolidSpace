using System.Collections.Generic;
using SolidSpace.UI;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    public interface ISpawnPointTool : IUIElement
    {
        IEnumerable<float2> OnUpdate();
    }
}