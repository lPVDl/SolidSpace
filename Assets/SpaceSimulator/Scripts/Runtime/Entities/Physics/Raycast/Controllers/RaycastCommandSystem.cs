using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class RaycastCommandSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;

        private readonly World _world;
        private readonly RaycastComputeSystem _computeSystem;

        public RaycastCommandSystem(World world, RaycastComputeSystem computeSystem)
        {
            _world = world;
            _computeSystem = computeSystem;
        }

        public void Initialize()
        {
            
        }

        public void Update()
        {
            var hitCount = _computeSystem.HitCount;
            var slice = new NativeSlice<Entity>(_computeSystem.HitEntities, 0, hitCount);
            _world.EntityManager.DestroyEntity(slice);

            SpaceDebug.LogState("RayHit", hitCount);
        }

        public void FinalizeSystem()
        {
            
        }
    }
}