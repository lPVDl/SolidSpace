using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Common
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;
    }
}