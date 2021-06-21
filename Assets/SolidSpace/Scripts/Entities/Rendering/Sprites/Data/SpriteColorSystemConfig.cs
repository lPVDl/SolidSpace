using SolidSpace.Entities.Rendering.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [System.Serializable]
    public class SpriteColorSystemConfig
    {
        public Atlas2DConfig AtlasConfig => _atlasConfig;
        public TextureFormat TextureFormat => _textureFormat;

        [SerializeField] private TextureFormat _textureFormat;
        [SerializeField] private Atlas2DConfig _atlasConfig;
    }
}