using SolidSpace.Profiling;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IColliderBakeSystemFactory
    {
        IColliderBakeSystem<T> Create<T>(ProfilingHandle profiler) where T : struct, IColliderBakeBehaviour;
    }
}