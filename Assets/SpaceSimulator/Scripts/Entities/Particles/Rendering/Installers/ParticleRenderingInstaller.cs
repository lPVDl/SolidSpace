using UnityEngine;

namespace SpaceSimulator.Entities.Particles.Rendering
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