using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics
{
    public class VelocitySystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCompute;
        
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;

        public VelocitySystem(IEntityManager entityManager, IEntityWorldTime time)
        {
            _entityManager = entityManager;
            _time = time;
        }
        
        public void Initialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
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
                deltaTime = _time.DeltaTime,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(false),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                chunks = chunks
            };
            
            job.Schedule(chunks.Length, 32).Complete();
        }

        public void FinalizeObject()
        {
            
        }
    }
}