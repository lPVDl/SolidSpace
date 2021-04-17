namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class ParticleEmissionInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ParticleEmitterComputeSystem>().AsSingle();
            Container.BindInterfacesTo<ParticleEmitterCommandSystem>().AsSingle();
        }
    }
}