using SolidSpace.Debugging;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = false, OrderLast = true)]
    public class DespawnCommandSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        
        private readonly IDespawnComputeSystem _computeSystem;
        private readonly IEntityManager _entityManager;

        public DespawnCommandSystem(IDespawnComputeSystem computeSystem, IEntityManager entityManager)
        {
            _computeSystem = computeSystem;
            _entityManager = entityManager;
        }
        
        public void InitializeController()
        {
            
        }

        public void UpdateController()
        {
            var slice = new NativeSlice<Entity>(_computeSystem.ResultBuffer, 0, _computeSystem.ResultCount);
            
            _entityManager.DestroyEntity(slice);
            
            SpaceDebug.LogState("DespawnCount", _computeSystem.ResultCount);
        }

        public void FinalizeController()
        {
            
        }
    }
}