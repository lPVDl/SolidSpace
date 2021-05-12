using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics
{
    public class VelocityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<VelocitySystem>();
        }
    }
}