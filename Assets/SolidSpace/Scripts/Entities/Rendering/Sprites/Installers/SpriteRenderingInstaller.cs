using SolidSpace.DependencyInjection;
using SolidSpace.Entities.Rendering.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    internal class SpriteRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private AtlasConfig _colorAtlasConfig;
        [SerializeField] private SpriteMeshSystemConfig _meshSystemConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SpriteColorSystem>(_colorAtlasConfig);
            container.Bind<SpriteMeshSystem>(_meshSystemConfig);
        }
    }
}