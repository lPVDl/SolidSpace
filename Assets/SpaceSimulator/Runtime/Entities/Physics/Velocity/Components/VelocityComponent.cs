using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}