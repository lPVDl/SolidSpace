using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private SandboxConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<SandboxController>(_config);
            container.Bind<CheckedButtonViewFactory>();
        }
    }
}