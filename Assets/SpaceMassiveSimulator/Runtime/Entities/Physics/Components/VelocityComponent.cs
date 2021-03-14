using Unity.Entities;
using Unity.Mathematics;

namespace SpaceMassiveSimulator.Runtime.Entities.Physics
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}