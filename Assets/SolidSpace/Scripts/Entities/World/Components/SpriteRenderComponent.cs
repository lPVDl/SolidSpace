using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.World
{
    public struct SpriteRenderComponent : IComponentData
    {
        public AtlasIndex index;
    }
}