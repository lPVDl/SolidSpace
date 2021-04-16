namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleRenderingInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ParticleMeshSystem>().AsSingle();
        }
    }
}