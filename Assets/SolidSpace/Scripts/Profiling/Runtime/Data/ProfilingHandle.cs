using System.Runtime.CompilerServices;

namespace SolidSpace.Profiling
{
    public readonly struct ProfilingHandle
    {
        private readonly ProfilingManager _manager;

        public ProfilingHandle(ProfilingManager manager)
        {
            _manager = manager;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginSample(string name)
        {
            _manager.OnBeginSample(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndSample(string name)
        {
            _manager.OnEndSample(name);
        }
    }
}