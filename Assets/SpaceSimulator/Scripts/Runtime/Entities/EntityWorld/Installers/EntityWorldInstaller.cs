using UnityEngine;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorldInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private EntityCycleConfig _entityCycleConfig;
        
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesTo<EntityCycleController>(_entityCycleConfig);
            container.BindInterfacesTo<EntityManager>();
            container.BindInterfacesTo<EntityWorldTime>();
        }
    }
}