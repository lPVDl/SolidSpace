namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesTo<RepeatTimerSystem>();
        }
    }
}