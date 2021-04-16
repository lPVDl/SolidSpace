namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RepeatTimerSystem>().AsSingle();
        }
    }
}