using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}