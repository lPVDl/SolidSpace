using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Eraser
{
    public class EraserToolInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private EraserToolConfig _config;
        
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<EraserTool>(_config);
        }
    }
}