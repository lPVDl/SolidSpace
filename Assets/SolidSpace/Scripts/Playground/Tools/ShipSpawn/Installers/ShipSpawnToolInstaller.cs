using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    internal class ShipSpawnToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ShipSpawnTool>();
        }
    }
}