using SolidSpace.DependencyInjection;
using SolidSpace.Playground.Core;
using UnityEngine;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class EmitterSpawnToolInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EmitterSpawnTool>();
        }
    }
}