using SolidSpace.DependencyInjection;
using SolidSpace.Playground.Core;
using UnityEngine;

namespace SolidSpace.Playground.Tools.VelocityChange
{
    public class VelocityChangeToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundToolConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<VelocityChangeTool>(_config);
        }
    }
}