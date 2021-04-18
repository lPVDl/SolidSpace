using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Entities
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }
}