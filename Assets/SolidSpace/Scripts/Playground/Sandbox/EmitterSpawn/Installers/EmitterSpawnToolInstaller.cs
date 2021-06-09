using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.EmitterSpawn
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