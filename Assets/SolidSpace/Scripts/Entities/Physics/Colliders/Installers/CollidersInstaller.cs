using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal class CollidersInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ColliderBakeSystemConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ColliderBakeSystem>(_config);
        }
    }
}