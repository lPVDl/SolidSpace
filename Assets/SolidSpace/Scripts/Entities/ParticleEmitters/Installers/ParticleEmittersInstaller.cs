namespace SolidSpace.Entities.ParticleEmitters
{
    public class ParticleEmittersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<ParticleEmitterComputeSystem>();
            container.Bind<ParticleEmitterCommandSystem>();
        }
    }
}