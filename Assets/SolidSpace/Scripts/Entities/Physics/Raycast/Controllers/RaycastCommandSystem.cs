using SolidSpace.Debugging;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics
{
    public class RaycastCommandSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;

        private readonly IEntityManager _entityManager;
        private readonly IRaycastComputeSystem _computeSystem;

        public RaycastCommandSystem(IEntityManager entityManager, IRaycastComputeSystem computeSystem)
        {
            _entityManager = entityManager;
            _computeSystem = computeSystem;
        }

        public void InitializeController()
        {
            
        }

        public void UpdateController()
        {
            var hitCount = _computeSystem.HitCount;
            var slice = new NativeSlice<Entity>(_computeSystem.HitEntities, 0, hitCount);
            _entityManager.DestroyEntity(slice);

            SpaceDebug.LogState("RayHit", hitCount);
        }

        public void FinalizeController()
        {
            
        }
    }
}