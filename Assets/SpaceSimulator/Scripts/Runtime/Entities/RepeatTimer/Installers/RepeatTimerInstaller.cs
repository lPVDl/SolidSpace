using Zenject;

namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<RepeatTimerSystem>().AsSingle();
        }
    }
}