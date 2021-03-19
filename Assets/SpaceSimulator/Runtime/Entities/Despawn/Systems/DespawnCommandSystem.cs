using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = false, OrderLast = true)]
    public class DespawnCommandSystem : SystemBase
    {
        private DespawnComputeSystem _computeSystem;
        
        protected override void OnCreate()
        {
            _computeSystem = World.GetOrCreateSystem<DespawnComputeSystem>();
        }

        protected override void OnUpdate()
        {
            if (!_computeSystem.Enabled)
            {
                return;
            }

            var slice = new NativeSlice<Entity>(_computeSystem.ResultBuffer, 0, _computeSystem.ResultCount);
            
            EntityManager.DestroyEntity(slice);
            
            SpaceDebug.LogState("DespawnCount", _computeSystem.ResultCount);
        }
    }
}