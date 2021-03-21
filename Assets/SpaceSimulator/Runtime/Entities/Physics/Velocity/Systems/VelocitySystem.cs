using SpaceSimulator.Runtime.Entities.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocitySystem : SystemBase
    {
        private EntityQuery _query;
        
        protected override void OnStartRunning()
        {
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent)
            });
        }

        protected override void OnUpdate()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new VelocityJob
            {
                deltaTime = Time.DeltaTime,
                positionHandle = GetComponentTypeHandle<PositionComponent>(),
                velocityHandle = GetComponentTypeHandle<VelocityComponent>(true),
                chunks = chunks
            };
            
            job.Schedule(chunks.Length, 32, Dependency).Complete();
        }
    }
}