using SolidSpace.Profiling;

namespace SolidSpace.Entities.Physics.Raycast
{
    public interface IRaycastSystemFactory
    {
        public IRaycastSystem<T> Create<T>(ProfilingHandle profiler) where T : struct, IRaycastBehaviour;
    }
}