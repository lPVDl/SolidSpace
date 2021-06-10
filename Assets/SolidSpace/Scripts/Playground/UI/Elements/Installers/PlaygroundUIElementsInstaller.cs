using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.UI.Elements
{
    internal class PlaygroundUIElementsInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ToolButtonFactory>();
            container.Bind<ToolWindowFactory>();
        }
    }
}