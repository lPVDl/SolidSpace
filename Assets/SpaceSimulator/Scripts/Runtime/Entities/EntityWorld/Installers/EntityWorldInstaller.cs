using Unity.Entities;
using UnityEngine;
using Zenject;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorldInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private EntityCycleConfig _entityCycleConfig;
        
        public override void InstallBindings()
        {
            var world = new World("SpaceSimulator");
            Container.BindInstance(world).AsSingle();
            Container.BindInterfacesTo<EntityCycleController>().AsSingle().WithArguments(_entityCycleConfig);
        }
    }
}