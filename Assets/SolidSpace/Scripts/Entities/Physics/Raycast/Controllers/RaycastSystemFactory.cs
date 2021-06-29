using SolidSpace.Entities.World;
using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public class RaycastSystemFactory : IRaycastSystemFactory
    {
        private readonly IEntityManager _entityManager;

        public RaycastSystemFactory(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public IRaycastSystem<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents)
            where T : struct, IRaycastBehaviour
        {
            return new RaycastSystem<T>
            {
                Profiler = profiler,
                Query = _entityManager.CreateEntityQuery(requiredComponents)
            };
        }
    }
}