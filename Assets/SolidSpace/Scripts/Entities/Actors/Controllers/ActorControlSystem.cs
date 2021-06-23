using SolidSpace.Entities.Actors.Interfaces;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Actors
{
    public class ActorControlSystem : IInitializable, IUpdatable, IActorControlSystem
    {
        private readonly IEntityWorldManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityWorldTime _worldTime;

        private EntityQuery _query;
        private float2 _targetPosition;
        private ProfilingHandle _profiler;

        public ActorControlSystem(IEntityWorldManager entityManager, IProfilingManager profilingManager, 
            IEntityWorldTime worldTime)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _worldTime = worldTime;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(RotationComponent),
                typeof(ActorComponent)
            });
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query chunks");
            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Job");
            new ActorControlJob
            {
                inArchetypeChunks = archetypeChunks,
                inDeltaTime = _worldTime.DeltaTime,
                inSeekPosition = _targetPosition,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(false),
                actorHandle = _entityManager.GetComponentTypeHandle<ActorComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(false)
                
            }.Schedule(archetypeChunks.Length, 4).Complete();
            _profiler.EndSample("Job");

            _profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void SetActorsTargetPosition(float2 position)
        {
            _targetPosition = position;
        }
        
        public void OnFinalize()
        {
            
        }
    }
}