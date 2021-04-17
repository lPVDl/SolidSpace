namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class ParticleEmissionInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesAndSelfTo<ParticleEmitterComputeSystem>();
            container.BindInterfacesTo<ParticleEmitterCommandSystem>();
        }
    }
}