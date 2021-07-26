using UnityEngine;

namespace SolidSpace.Profiling
{
    [System.Serializable]
    public class ProfilingConfig
    {
        public bool EnableSolidProfiling => _enableSolidProfiling;
        public bool EnableUnityProfiling => _enableUnityProfiling;
        public int MaxRecordCount => _maxRecordCount;
        public int StackSize => _stackSize;
        public int WindowItemCount => _windowItemCount;
        public int WindowScrollMultiplier => _windowScrollMultiplier;
        
        [SerializeField] private bool _enableSolidProfiling;
        [SerializeField] private bool _enableUnityProfiling;
        [SerializeField] private int _maxRecordCount;
        [SerializeField] private int _stackSize;
        [SerializeField] private int _windowItemCount;
        [SerializeField] private int _windowScrollMultiplier;

        public ProfilingConfig(bool enableSolidProfiling, bool enableUnityProfiling, int maxRecordCount, int stackSize)
        {
            _enableSolidProfiling = enableSolidProfiling;
            _enableUnityProfiling = enableUnityProfiling;
            _maxRecordCount = maxRecordCount;
            _stackSize = stackSize;
        }
    }
}