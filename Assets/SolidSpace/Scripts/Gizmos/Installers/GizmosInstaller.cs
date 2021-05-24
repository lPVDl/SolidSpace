using SolidSpace.DependencyInjection;

namespace SolidSpace.Gizmos
{
    internal class GizmosInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<GizmosManager>();
        }
    }
}