using SpaceSimulator.Entities.Rendering.Atlases;
using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Sprites
{
    public class SpriteRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private AtlasConfig _colorAtlasConfig;
        public override void InstallBindings(IContainer container)
        {
            container.Bind<SpriteColorSystem>(_colorAtlasConfig);
        }
    }
}