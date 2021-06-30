using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public class RigidbodyComputeSystem : IUpdatable, IInitializable
    {
        private const float MotionSpeed = 100f;
        private const int CollisionStackSize = 32;
        
        private readonly IColliderBakeSystemFactory _colliderBakeSystemFactory;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _worldTime;

        private ProfilingHandle _profiler;
        private IColliderBakeSystem<RigidbodyColliderBakeBehaviour> _colliderBakeSystem;
        private EntityQuery _query;
        
        public RigidbodyComputeSystem(IColliderBakeSystemFactory colliderBakeSystemFactory, IProfilingManager profilingManager,
            IEntityManager entityManager, IEntityWorldTime worldTime)
        {
            _colliderBakeSystemFactory = colliderBakeSystemFactory;
            _profilingManager = profilingManager;
            _entityManager = entityManager;
            _worldTime = worldTime;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RectColliderComponent),
                typeof(RectSizeComponent),
                typeof(RigidbodyComponent)
            });
            _colliderBakeSystem = _colliderBakeSystemFactory.Create<RigidbodyColliderBakeBehaviour>(_profiler);
        }
        
        public void OnUpdate()
        {
            var bakeBehaviour = new RigidbodyColliderBakeBehaviour
            {
                rigidbodyHandle = _entityManager.GetComponentTypeHandle<RigidbodyComponent>(false) 
            };
            
            _profiler.BeginSample("Query chunks");
            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Bake colliders");
            var colliders = _colliderBakeSystem.Bake(archetypeChunks, ref bakeBehaviour);
            _profiler.EndSample("Bake colliders");
            
            _profiler.BeginSample("Collision job");
            var collisionJob = new RigidbodyCollisionJob
            {
                inMotionHalfSpeed = MotionSpeed * _worldTime.DeltaTime * 0.5f,
                rigidbodyHandle = _entityManager.GetComponentTypeHandle<RigidbodyComponent>(true),
                inArchetypeChunks = archetypeChunks,
                inColliders = colliders,
                hitStack = NativeMemory.CreateTempJobArray<ushort>(archetypeChunks.Length * CollisionStackSize),
                hitStackSize = CollisionStackSize,
                outMotion = NativeMemory.CreateTempJobArray<float2>(colliders.shapes.Length)
            };
            collisionJob.Schedule(archetypeChunks.Length, 1).Complete();
            _profiler.EndSample("Collision job");

            _profiler.BeginSample("Motion job");
            new RigidbodyMotionJob
            {
                inMotion = collisionJob.outMotion,
                inArchetypeChunks = archetypeChunks,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(false),
                rigidbodyHandle = _entityManager.GetComponentTypeHandle<RigidbodyComponent>(true),
            }.Schedule(archetypeChunks.Length, 2).Complete();
            _profiler.EndSample("Motion job");
            
            _profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            colliders.Dispose();
            collisionJob.hitStack.Dispose();
            collisionJob.outMotion.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void OnFinalize()
        {
            
        }
    }
}