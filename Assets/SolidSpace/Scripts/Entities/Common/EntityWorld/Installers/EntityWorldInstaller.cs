using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities
{
    public class EntityWorldInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EntityManager>();
            container.Bind<EntityWorldTime>();
        }
    }
}