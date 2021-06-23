using SolidSpace.DependencyInjection;

namespace SolidSpace.Playground.Tools.ActorControl
{
    public class ActorControlToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ActorControlTool>();
        }
    }
}