using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    public class SpawnPointToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SpawnPointToolFactory>();
        }
    }
}