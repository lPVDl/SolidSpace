using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }
}