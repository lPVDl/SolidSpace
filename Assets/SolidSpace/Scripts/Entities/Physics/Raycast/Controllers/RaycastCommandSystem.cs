using SolidSpace.Debugging;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    internal class RaycastCommandSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;

        private readonly IEntityWorldManager _entityManager;
        private readonly IRaycastComputeSystem _computeSystem;

        public RaycastCommandSystem(IEntityWorldManager entityManager, IRaycastComputeSystem computeSystem)
        {
            _entityManager = entityManager;
            _computeSystem = computeSystem;
        }

        public void InitializeController()
        {
            
        }

        public void UpdateController()
        {
            var hits = _computeSystem.RaycastWorld.hits;
            for (var i = 0; i < hits.Length; i++)
            {
                _entityManager.DestroyEntity(hits[i].raycasterEntity);
            }
            
            SpaceDebug.LogState("RayHit", hits.Length);
        }

        public void FinalizeController()
        {
            
        }
    }
}