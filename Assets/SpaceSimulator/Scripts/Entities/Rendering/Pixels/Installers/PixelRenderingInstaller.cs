using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Pixels
{
    public class PixelRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PixelMeshSystemConfig _pixelMeshSystemConfig;
        
        public override void InstallBindings(IContainer container)
        {
            container.Bind<PixelMeshSystem>(_pixelMeshSystemConfig);
        }
    }
}