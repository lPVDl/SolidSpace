using UnityEngine;

namespace SpaceSimulator.Entities.SpriteRendering
{
    public class SpriteRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private SpriteAtlasConfig _colorAtlasConfig;
        public override void InstallBindings(IContainer container)
        {
            container.Bind<SpriteAtlasColorSystem>(_colorAtlasConfig);
        }
    }
}