using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.UI.Factory
{
    internal class PlaygroundUIFactoryInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundUIFactoryConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PlaygroundUIFactory>(_config);
        }
    }
}