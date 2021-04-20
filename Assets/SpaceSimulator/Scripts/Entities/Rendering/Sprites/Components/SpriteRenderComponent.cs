using SpaceSimulator.Entities.Rendering.Atlases;
using Unity.Entities;

namespace SpaceSimulator.Entities.Rendering.Sprites
{
    public struct SpriteRenderComponent : IComponentData
    {
        public AtlasIndex colorIndex;
        public byte sizeX;
        public byte sizeY;
    }
}