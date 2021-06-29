using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public interface IRaycastSystemFactory
    {
        public IRaycastSystem<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents)
            where T : struct, IRaycastBehaviour;
    }
}