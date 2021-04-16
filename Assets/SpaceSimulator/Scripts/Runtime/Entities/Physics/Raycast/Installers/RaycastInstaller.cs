namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class RaycastInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RaycastComputeSystem>().AsSingle();
            Container.BindInterfacesTo<RaycastCommandSystem>().AsSingle();
        }
    }
}