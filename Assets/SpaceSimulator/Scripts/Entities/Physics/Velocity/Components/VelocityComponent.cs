using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Entities.Physics
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}