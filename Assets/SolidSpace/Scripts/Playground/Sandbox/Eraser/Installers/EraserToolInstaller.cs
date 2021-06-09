using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.Eraser
{
    public class EraserToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private EraserToolConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EraserTool>(_config);
            container.Bind<EntityByPositionSearchSystem>();
        }
    }
}