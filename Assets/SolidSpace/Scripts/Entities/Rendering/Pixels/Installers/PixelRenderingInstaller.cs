using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Pixels
{
    public class PixelRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PixelMeshSystemConfig _pixelMeshSystemConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PixelMeshSystem>(_pixelMeshSystemConfig);
        }
    }
}