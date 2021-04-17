namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RepeatTimerSystem>().AsSingle();
        }
    }
}