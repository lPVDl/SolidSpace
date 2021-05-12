using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics
{
    public class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ColliderBakeSystem>();
        }
    }
}