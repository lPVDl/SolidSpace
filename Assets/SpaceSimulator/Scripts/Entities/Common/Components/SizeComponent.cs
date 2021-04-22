using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Entities
{
    public struct SizeComponent : IComponentData
    {
        public half2 value;
    }
}