namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DespawnComputeSystem>().AsSingle();
            Container.BindInterfacesTo<DespawnCommandSystem>().AsSingle();
        }
    }
}