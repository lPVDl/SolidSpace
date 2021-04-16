namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class ParticleEmissionInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ParticleEmitterComputeSystem>().AsSingle();
            Container.BindInterfacesTo<ParticleEmitterCommandSystem>().AsSingle();
        }
    }
}