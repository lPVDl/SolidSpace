using SolidSpace.Entities.Rendering.Atlases;
using Unity.Entities;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public struct SpriteRenderComponent : IComponentData
    {
        public AtlasIndex colorIndex;
    }
}