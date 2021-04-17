using UnityEngine;
using Zenject;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorldInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private EntityCycleConfig _entityCycleConfig;
        
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<EntityCycleController>().AsSingle().WithArguments(_entityCycleConfig);
            container.BindInterfacesTo<EntityManager>().AsSingle();
            container.BindInterfacesTo<EntityWorldTime>().AsSingle();
        }
    }
}