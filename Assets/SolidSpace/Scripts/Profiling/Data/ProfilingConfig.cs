using UnityEngine;

namespace SolidSpace.Profiling
{
    [System.Serializable]
    public class ProfilingConfig
    {
        public bool EnableSolidProfiling => _enableSolidProfiling;
        public bool EnableUnityProfiling => _enableUnityProfiling;
        
        [SerializeField] private bool _enableSolidProfiling;
        [SerializeField] private bool _enableUnityProfiling;

        public ProfilingConfig(bool enableSolidProfiling, bool enableUnityProfiling)
        {
            _enableSolidProfiling = enableSolidProfiling;
            _enableUnityProfiling = enableUnityProfiling;
        }
    }
}