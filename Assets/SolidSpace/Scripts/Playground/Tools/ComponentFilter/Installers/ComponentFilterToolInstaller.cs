using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ComponentFilterMasterConfig _masterConfig;
        [SerializeField] private ComponentFilterWindowConfig _windowConfig;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<ComponentFilterMaster>(_masterConfig);
            container.Bind<ComponentFilterWindow>(_windowConfig);
        }
    }
}