namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleRenderingInstaller : ScriptableInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ParticleMeshSystem>().AsSingle();
        }
    }
}