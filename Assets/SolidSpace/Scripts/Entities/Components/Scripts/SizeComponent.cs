using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Components
{
    public struct SizeComponent : IComponentData
    {
        public half2 value;
    }
}