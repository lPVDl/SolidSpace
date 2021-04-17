using Zenject;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class ParticleEmissionInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ParticleEmitterComputeSystem>().AsSingle();
            container.BindInterfacesTo<ParticleEmitterCommandSystem>().AsSingle();
        }
    }
}