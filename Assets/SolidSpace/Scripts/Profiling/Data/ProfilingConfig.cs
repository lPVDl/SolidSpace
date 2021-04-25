using UnityEngine;

namespace SolidSpace.Profiling.Data
{
    [System.Serializable]
    public class ProfilingConfig
    {
        public bool EnableSolidProfiling => _enableSolidProfiling;
        public bool EnableUnityProfiling => _enableUnityProfiling;
        
        [SerializeField] private bool _enableSolidProfiling;
        [SerializeField] private bool _enableUnityProfiling;
    }
}