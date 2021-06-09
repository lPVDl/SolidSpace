using System.Collections.Generic;
using Sirenix.OdinInspector;
using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    internal class PlaygroundCoreInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundCoreConfig _config;
        [SerializeField, InlineEditor] private List<ScriptableObjectInstaller> _toolInstallers;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.BindFromComponentInHierarchy<Camera>();
            container.Bind<PlaygroundCoreController>(_config);
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