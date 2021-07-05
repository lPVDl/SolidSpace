using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.UI.Factory
{
    internal class UIFactoryInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private UIPrefabs _prefabs;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<UIFactory>(_prefabs);

            container.Bind<ToolButtonFactory>();
            container.Bind<ToolWindowFactory>();
            container.Bind<TagLabelFactory>();
            container.Bind<LayoutGridFactory>();
            container.Bind<GeneralButtonFactory>();
            container.Bind<StringFieldFactory>();
        }
    }
}