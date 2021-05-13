using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Despawn
{
    internal class DespawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<DespawnComputeSystem>();
            container.Bind<DespawnCommandSystem>();
        }
    }
}