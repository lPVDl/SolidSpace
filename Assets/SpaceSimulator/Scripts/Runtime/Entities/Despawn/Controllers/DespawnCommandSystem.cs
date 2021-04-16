using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = false, OrderLast = true)]
    public class DespawnCommandSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;
        
        private readonly DespawnComputeSystem _computeSystem;
        private readonly IEntityWorld _world;

        public DespawnCommandSystem(DespawnComputeSystem computeSystem, IEntityWorld world)
        {
            _computeSystem = computeSystem;
            _world = world;
        }
        
        public void Initialize()
        {
            
        }

        public void Update()
        {
            var slice = new NativeSlice<Entity>(_computeSystem.ResultBuffer, 0, _computeSystem.ResultCount);
            
            _world.EntityManager.DestroyEntity(slice);
            
            SpaceDebug.LogState("DespawnCount", _computeSystem.ResultCount);
        }

        public void FinalizeSystem()
        {
            
        }
    }
}