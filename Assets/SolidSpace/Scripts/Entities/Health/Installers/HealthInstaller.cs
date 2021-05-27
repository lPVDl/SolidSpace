using SolidSpace.DependencyInjection;
using SolidSpace.Entities.Health.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public class HealthInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private LinearAtlasConfig _healthAtlasConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            
        }
    }
}