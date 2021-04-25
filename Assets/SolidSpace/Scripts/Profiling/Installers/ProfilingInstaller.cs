using UnityEngine;

namespace SolidSpace.Profiling
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