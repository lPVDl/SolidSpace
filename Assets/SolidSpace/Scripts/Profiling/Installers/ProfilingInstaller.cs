using SolidSpace.Profiling.Controllers;
using SolidSpace.Profiling.Data;
using UnityEngine;

namespace SolidSpace.Profiling.Installers
{
    public class ProfilingInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ProfilingConfig _config;
        
        public override void InstallBindings(IContainer container)
        {
            container.Bind<ProfilingManager>(_config);
        }
    }
}