using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ColliderBakeSystem>();
        }
    }
}