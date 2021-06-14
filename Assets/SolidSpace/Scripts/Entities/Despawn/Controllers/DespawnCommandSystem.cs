using SolidSpace.Debugging;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    internal class DespawnCommandSystem : IUpdatable
    {
        private readonly IDespawnComputeSystem _computeSystem;
        private readonly IEntityWorldManager _entityManager;

        public DespawnCommandSystem(IDespawnComputeSystem computeSystem, IEntityWorldManager entityManager)
        {
            _computeSystem = computeSystem;
            _entityManager = entityManager;
        }

        public void Update()
        {
            var slice = new NativeSlice<Entity>(_computeSystem.ResultBuffer, 0, _computeSystem.ResultCount);
            
            _entityManager.DestroyEntity(slice);
            
            SpaceDebug.LogState("DespawnCount", _computeSystem.ResultCount);
        }
    }
}