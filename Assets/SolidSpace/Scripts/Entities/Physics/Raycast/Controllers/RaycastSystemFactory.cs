using SolidSpace.Entities.World;
using SolidSpace.Profiling;

namespace SolidSpace.Entities.Physics.Raycast
{
    public class RaycastSystemFactory : IRaycastSystemFactory
    {
        private readonly IEntityManager _entityManager;

        public RaycastSystemFactory(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public IRaycastSystem<T> Create<T>(ProfilingHandle profiler) where T : struct, IRaycastBehaviour
        {
            return new RaycastSystem<T>
            {
                Profiler = profiler
            };
        }
    }
}