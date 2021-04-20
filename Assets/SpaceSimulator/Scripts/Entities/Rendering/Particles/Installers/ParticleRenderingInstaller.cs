using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Particles
{
    public class ParticleRenderingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ParticleMeshSystemConfig _particleMeshSystemConfig;
        
        public override void InstallBindings(IContainer container)
        {
            container.Bind<ParticleMeshSystem>(_particleMeshSystemConfig);
        }
    }
}