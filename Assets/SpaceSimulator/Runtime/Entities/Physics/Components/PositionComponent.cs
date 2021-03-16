using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }
}