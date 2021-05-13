using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics
{
    internal class RaycastInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<RaycastComputeSystem>();
            container.Bind<RaycastCommandSystem>();
        }
    }
}