using SolidSpace.Debugging;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics
{
    public class RaycastCommandSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;

        private readonly IEntityManager _entityManager;
        private readonly IRaycastComputeSystem _computeSystem;

        public RaycastCommandSystem(IEntityManager entityManager, IRaycastComputeSystem computeSystem)
        {
            _entityManager = entityManager;
            _computeSystem = computeSystem;
        }

        public void Initialize()
        {
            
        }

        public void Update()
        {
            var hitCount = _computeSystem.HitCount;
            var slice = new NativeSlice<Entity>(_computeSystem.HitEntities, 0, hitCount);
            _entityManager.DestroyEntity(slice);

            SpaceDebug.LogState("RayHit", hitCount);
        }

        public void FinalizeSystem()
        {
            
        }
    }
}