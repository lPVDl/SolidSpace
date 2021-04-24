using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities
{
    public struct SizeComponent : IComponentData
    {
        public half2 value;
    }
}