using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.ImageSpawn
{
    public class ImageSpawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ImageSpawnTool>();
        }
    }
}