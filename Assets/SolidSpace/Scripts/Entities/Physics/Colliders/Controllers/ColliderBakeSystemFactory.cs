using SolidSpace.Entities.World;
using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public class ColliderBakeSystemFactory : IColliderBakeSystemFactory
    {
        private readonly IEntityManager _entityManager;

        public ColliderBakeSystemFactory(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public IColliderBakeSystem<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents) 
            where T : struct, IColliderBakeBehaviour
        {
            return new ColliderBakeSystem<T>
            {
                EntityManager = _entityManager,
                Query = _entityManager.CreateEntityQuery(requiredComponents),
                Profiler = profiler
            };
        }
    }
}