using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.ShipSpawn
{
    internal class ShipSpawnToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ShipSpawnToolConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ShipSpawnTool>(_config);
        }
    }
}