namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesAndSelfTo<DespawnComputeSystem>();
            container.BindInterfacesTo<DespawnCommandSystem>();
        }
    }
}