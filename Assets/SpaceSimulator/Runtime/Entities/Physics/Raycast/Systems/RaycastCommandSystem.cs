using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Physics.Raycast
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = false, OrderLast = true)]
    public class RaycastCommandSystem : SystemBase
    {
        private RaycastComputeSystem _computeSystem;
        
        protected override void OnCreate()
        {
            _computeSystem = World.GetOrCreateSystem<RaycastComputeSystem>();
        }

        protected override void OnUpdate()
        {
            if (!_computeSystem.Enabled)
            {
                return;
            }

            var hitCount = _computeSystem.HitCount;
            var slice = new NativeSlice<Entity>(_computeSystem.HitEntities, 0, hitCount);
            EntityManager.DestroyEntity(slice);

            SpaceDebug.LogState("RayHit", hitCount);
        }
    }
}