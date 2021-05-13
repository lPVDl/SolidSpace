using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.World
{
    public struct RotationComponent : IComponentData
    {
        public half value;
    }
}