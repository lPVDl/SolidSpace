using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics.Velcast
{
    internal class VelcastInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<VelcastComputeSystem>();
            container.Bind<KovacRaycasterFactory>();
        }
    }
}