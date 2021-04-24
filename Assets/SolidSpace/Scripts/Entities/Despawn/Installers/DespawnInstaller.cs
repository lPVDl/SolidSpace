namespace SolidSpace.Entities.Despawn
{
    public class DespawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<DespawnComputeSystem>();
            container.Bind<DespawnCommandSystem>();
        }
    }
}