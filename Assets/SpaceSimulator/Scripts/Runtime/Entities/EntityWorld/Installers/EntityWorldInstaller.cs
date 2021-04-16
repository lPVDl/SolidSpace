using UnityEngine;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorldInstaller : Installer
    {
        [SerializeField] private EntityCycleConfig _entityCycleConfig;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<EntityCycleController>().AsSingle().WithArguments(_entityCycleConfig);
            Container.BindInterfacesTo<EntityWorld>().AsSingle();
        }
    }
}