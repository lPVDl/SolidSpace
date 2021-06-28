using SolidSpace.Entities.World;
using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public class KovacFactory : IKovacFactory
    {
        private readonly IEntityManager _entityManager;

        public KovacFactory(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public IKovacBakery<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents) 
            where T : struct, IColliderWorld
        {
            return new KovacBakery<T>
            {
                EntityManager = _entityManager,
                Query = _entityManager.CreateEntityQuery(requiredComponents),
                Profiler = profiler
            };
        }
    }
}