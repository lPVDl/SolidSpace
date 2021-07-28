using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Parenting
{
    public class ParentingInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ParentHandleManager>();
            container.Bind<ParentHandleGarbageCollector>();
        }
    }
}