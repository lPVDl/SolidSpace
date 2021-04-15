namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : ScriptableInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RepeatTimerSystem>().AsSingle();
        }
    }
}