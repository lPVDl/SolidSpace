using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities
{
    public class EntityWorldInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private EntityCycleConfig _entityCycleConfig;

        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EntityCycleController>(_entityCycleConfig);
            container.Bind<EntityManager>();
            container.Bind<EntityWorldTime>();
        }
    }
}