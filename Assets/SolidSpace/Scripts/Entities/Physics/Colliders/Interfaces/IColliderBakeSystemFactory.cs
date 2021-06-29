using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IColliderBakeSystemFactory
    {
        IColliderBakeSystem<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents)
            where T : struct, IColliderBakeBehaviour;
    }
}