using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Compute;

        private readonly IEntityWorld _world;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;

        public RepeatTimerSystem(IEntityWorld world, IEntityWorldTime time)
        {
            _world = world;
            _time = time;
        }
        
        public void Initialize()
        {
            _query = _world.EntityManager.CreateEntityQuery(typeof(RepeatTimerComponent));
        }

        public void Update()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new RepeatTimerJob
            {
                chunks = chunks,
                deltaTime = _time.DeltaTime,
                timerHandle = _world.EntityManager.GetComponentTypeHandle<RepeatTimerComponent>(false)
            };
            
            SpaceDebug.LogState("deltaTime", _time.DeltaTime);
            
            var handle = job.Schedule(chunks.Length, 32);
            handle.Complete();

            chunks.Dispose();
        }

        public void FinalizeSystem()
        {
            
        }
    }
}