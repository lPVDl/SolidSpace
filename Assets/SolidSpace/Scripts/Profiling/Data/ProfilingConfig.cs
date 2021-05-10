using UnityEngine;

namespace SolidSpace.Profiling
{
    [System.Serializable]
    public class ProfilingConfig
    {
        public bool EnableSolidProfiling => _enableSolidProfiling;
        public bool EnableUnityProfiling => _enableUnityProfiling;
        public int MaxRecordCount => _maxRecordCount;
        
        [SerializeField] private bool _enableSolidProfiling;
        [SerializeField] private bool _enableUnityProfiling;
        [SerializeField] private int _maxRecordCount;

        public ProfilingConfig(bool enableSolidProfiling, bool enableUnityProfiling, int maxRecordCount)
        {
            _enableSolidProfiling = enableSolidProfiling;
            _enableUnityProfiling = enableUnityProfiling;
            _maxRecordCount = maxRecordCount;
        }
    }
}