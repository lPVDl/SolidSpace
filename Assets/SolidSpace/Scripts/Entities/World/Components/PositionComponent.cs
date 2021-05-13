using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.World
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }
}