using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct SpriteRenderComponent : IComponentData
    {
        public AtlasIndex index;
    }
}