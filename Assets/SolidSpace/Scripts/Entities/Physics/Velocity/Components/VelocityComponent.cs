using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}