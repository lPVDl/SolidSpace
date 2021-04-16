using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocitySystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Compute;
        
        private readonly IEntityWorld _world;
        
        private EntityQuery _query;

        public VelocitySystem(IEntityWorld world)
        {
            _world = world;
        }
        
        public void Initialize()
        {
            _query = _world.EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent)
            });
        }

        public void Update()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new VelocityJob
            {
                deltaTime = _world.Time.DeltaTime,
                positionHandle = _world.EntityManager.GetComponentTypeHandle<PositionComponent>(false),
                velocityHandle = _world.EntityManager.GetComponentTypeHandle<VelocityComponent>(true),
                chunks = chunks
            };
            
            job.Schedule(chunks.Length, 32).Complete();
        }

        public void FinalizeSystem()
        {
            
        }
    }
}