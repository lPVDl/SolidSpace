using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ColliderBakeSystemFactory>();
        }
    }
}