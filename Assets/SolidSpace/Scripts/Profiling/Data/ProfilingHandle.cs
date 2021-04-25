using System.Runtime.CompilerServices;
using SolidSpace.Profiling.Interfaces;

namespace SolidSpace.Profiling.Data
{
    public readonly struct ProfilingHandle
    {
        private readonly IProfilingHandler _handler;

        public ProfilingHandle(IProfilingHandler handler)
        {
            _handler = handler;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BeginSample(string name)
        {
            _handler.OnBeginSample(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndSample(string name)
        {
            _handler.OnEndSample(name);
        }
    }
}