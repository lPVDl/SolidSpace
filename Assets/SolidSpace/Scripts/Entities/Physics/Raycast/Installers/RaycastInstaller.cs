using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics
{
    public class RaycastInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<RaycastComputeSystem>();
            container.Bind<RaycastCommandSystem>();
        }
    }
}