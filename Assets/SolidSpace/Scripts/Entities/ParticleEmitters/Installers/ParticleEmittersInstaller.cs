using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.ParticleEmitters
{
    public class ParticleEmittersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ParticleEmitterComputeSystem>();
            container.Bind<ParticleEmitterCommandSystem>();
        }
    }
}