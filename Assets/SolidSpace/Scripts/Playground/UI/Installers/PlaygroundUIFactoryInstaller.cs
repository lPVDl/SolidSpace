using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.UI
{
    internal class PlaygroundUIFactoryInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private PlaygroundUIPrefabs _prefabs;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<PlaygroundUIFactory>(_prefabs);
            
            container.Bind<ToolButtonFactory>();
            container.Bind<ToolWindowFactory>();
            container.Bind<TagLabelFactory>();
        }
    }
}