using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Entities.SearchNearestEntity
{
    public class SearchNearestEntityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SearchNearestEntitySystem>();
        }
    }
}