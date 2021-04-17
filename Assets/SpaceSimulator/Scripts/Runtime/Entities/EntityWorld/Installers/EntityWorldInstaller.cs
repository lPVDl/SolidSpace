namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorldInstaller : ScriptableObjectInstaller
    {
        [Serialize] private EntityCycleConfig _entityCycleConfig;
        
        public override void InstallBindings(IContainer container)
        {
            container.Bind<EntityCycleController>(_entityCycleConfig);
            container.Bind<EntityManager>();
            container.Bind<EntityWorldTime>();
        }
    }
}