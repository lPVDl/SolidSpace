namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DespawnComputeSystem>().AsSingle();
            Container.BindInterfacesTo<DespawnCommandSystem>().AsSingle();
        }
    }
}