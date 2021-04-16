namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class CollidersInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ColliderBakeSystem>().AsSingle();
        }
    }
}