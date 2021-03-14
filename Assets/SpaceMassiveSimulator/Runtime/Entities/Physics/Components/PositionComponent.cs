using Unity.Entities;
using Unity.Mathematics;

namespace SpaceMassiveSimulator.Runtime.Entities.Physics
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }
}