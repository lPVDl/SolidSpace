using SolidSpace.DependencyInjection;
using SolidSpace.Playground.Core;
using UnityEngine;

namespace SolidSpace.Playground.Tools.PositionChange
{
    public class PositionChangeToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundToolConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PositionChangeTool>(_config);
        }
    }
}