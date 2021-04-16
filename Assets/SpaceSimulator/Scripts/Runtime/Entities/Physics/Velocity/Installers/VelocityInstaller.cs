namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocityInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<VelocitySystem>().AsSingle();
        }
    }
}