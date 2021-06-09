using System.Collections.Generic;
using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private SandboxConfig _config;
        [SerializeField] private List<ScriptableObjectInstaller> _toolInstallers;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SandboxController>(_config);
            container.Bind<MouseTracker>();
            
            container.Bind<ToolButtonViewFactory>();
            container.Bind<ToolWindowViewFactory>();

            foreach (var toolInstaller in _toolInstallers)
            {
                toolInstaller.InstallBindings(container);
            }
        }
    }
}