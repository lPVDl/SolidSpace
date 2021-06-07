using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.UI.Installers
{
    internal class UIInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private UIConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<UIManager>(_config);
        }
    }
}