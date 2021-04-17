namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleRenderingInstaller : ScriptableObjectInstaller
    {
        [Serialize] public ParticleMeshSystemConfig _particleMeshSystemConfig;
        
        public override void InstallBindings(IContainer container)
        {
            container.Bind<ParticleMeshSystem>(_particleMeshSystemConfig);
        }
    }
}