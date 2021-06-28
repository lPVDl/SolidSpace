using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Velcast
{
    public interface IKovacRaycasterFactory
    {
        public IKovacRaycaster<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents)
            where T : struct, IRaycastBehaviour;
    }
}