using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities
{
    public struct RotationComponent : IComponentData
    {
        public half value;
    }
}