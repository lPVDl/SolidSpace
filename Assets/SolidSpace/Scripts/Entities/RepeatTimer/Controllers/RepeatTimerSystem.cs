using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.RepeatTimer
{
    public class RepeatTimerSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCompute;

        private readonly IEntityWorldManager _entityManager;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;

        public RepeatTimerSystem(IEntityWorldManager entityManager, IEntityWorldTime time)
        {
            _entityManager = entityManager;
            _time = time;
        }
        
        public void InitializeController()
        {
            _query = _entityManager.CreateEntityQuery(typeof(RepeatTimerComponent));
        }

        public void UpdateController()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new RepeatTimerJob
            {
                chunks = chunks,
                deltaTime = _time.DeltaTime,
                timerHandle = _entityManager.GetComponentTypeHandle<RepeatTimerComponent>(false)
            };
            
            SpaceDebug.LogState("deltaTime", _time.DeltaTime);
            
            var handle = job.Schedule(chunks.Length, 32);
            handle.Complete();

            chunks.Dispose();
        }

        public void FinalizeController()
        {
            
        }
    }
}