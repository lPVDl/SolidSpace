using SolidSpace.Entities.Rendering.Atlases;
using SolidSpace.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public interface ISpriteColorSystem : ITextureAtlas
    {
        public void Copy(Texture2D source, AtlasIndex target);
    }
}