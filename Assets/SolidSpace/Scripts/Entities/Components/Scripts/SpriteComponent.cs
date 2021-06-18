using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct SpriteComponent : IComponentData
    {
        public AtlasIndex index;
    }
}