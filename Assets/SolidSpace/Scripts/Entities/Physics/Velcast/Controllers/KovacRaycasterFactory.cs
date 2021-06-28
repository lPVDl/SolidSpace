using SolidSpace.Entities.World;
using SolidSpace.Profiling;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Velcast
{
    public class KovacRaycasterFactory : IKovacRaycasterFactory
    {
        private readonly IEntityManager _entityManager;

        public KovacRaycasterFactory(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public IKovacRaycaster<T> Create<T>(ProfilingHandle profiler, params ComponentType[] requiredComponents)
            where T : struct, IRaycastBehaviour
        {
            return new KovacRaycaster<T>
            {
                Profiler = profiler,
                Query = _entityManager.CreateEntityQuery(requiredComponents)
            };
        }
    }
}