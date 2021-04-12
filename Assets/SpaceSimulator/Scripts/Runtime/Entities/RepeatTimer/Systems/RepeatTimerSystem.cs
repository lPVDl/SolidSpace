using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public class RepeatTimerSystem : SystemBase
    {
        private EntityQuery _query;
        
        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(typeof(RepeatTimerComponent));
        }

        protected override void OnUpdate()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new RepeatTimerJob
            {
                chunks = chunks,
                deltaTime = Time.DeltaTime,
                timerHandle = GetComponentTypeHandle<RepeatTimerComponent>()
            };
            
            SpaceDebug.LogState("deltaTime", Time.DeltaTime);
            
            var handle = job.Schedule(chunks.Length, 32, Dependency);
            handle.Complete();

            chunks.Dispose();
        }
    }
}