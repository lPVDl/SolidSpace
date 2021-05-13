using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.World
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}