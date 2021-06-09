using SolidSpace.DependencyInjection;
using SolidSpace.Playground.Core;
using UnityEngine;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class EmitterSpawnToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundToolConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EmitterSpawnTool>(_config);
        }
    }
}