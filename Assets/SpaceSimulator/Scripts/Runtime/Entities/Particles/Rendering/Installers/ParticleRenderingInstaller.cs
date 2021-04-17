namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleRenderingInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<ParticleMeshSystem>();
        }
    }
}