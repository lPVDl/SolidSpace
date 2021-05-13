using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Components
{
    public struct RotationComponent : IComponentData
    {
        public half value;
    }
}