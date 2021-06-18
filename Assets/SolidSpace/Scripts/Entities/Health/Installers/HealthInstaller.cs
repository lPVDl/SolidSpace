using SolidSpace.DependencyInjection;
using SolidSpace.Entities.Health.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    internal class HealthInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private LinearAtlasConfig _healthAtlasConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<HealthAtlasSystem>(_healthAtlasConfig);
            container.Bind<HealthAtlasGarbageCollector>();
        }
    }
}