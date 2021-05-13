using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.World
{
    public struct SizeComponent : IComponentData
    {
        public half2 value;
    }
}