using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal class CollidersInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ColliderGizmoSystemConfig _gizmoConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ColliderBakeSystem>();
            container.Bind<ColliderGizmoSystem>(_gizmoConfig);
        }
    }
}