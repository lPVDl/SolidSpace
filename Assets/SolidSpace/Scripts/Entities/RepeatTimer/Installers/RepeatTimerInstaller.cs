namespace SolidSpace.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<RepeatTimerSystem>();
        }
    }
}