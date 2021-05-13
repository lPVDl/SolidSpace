using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics
{
    public class RaycastComputeSystem : IController, IRaycastComputeSystem
    {
        private const int EntityBufferChunkSize = 4096;

        public EControllerType ControllerType => EControllerType.EntityCompute;
        
        public NativeArray<Entity> HitEntities => _entityBuffer;
        public int HitCount => _entityCount[0];

        private readonly IEntityManager _entityManager;
        private readonly IColliderBakeSystem _colliderSystem;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _raycasterQuery;
        private NativeArray<Entity> _entityBuffer;
        private NativeArray<int> _entityCount;
        private NativeArrayUtil _arrayUtil;
        private ProfilingHandle _profiler;

        public RaycastComputeSystem(IEntityManager entityManager, IColliderBakeSystem colliderSystem,
            IEntityWorldTime time, IProfilingManager profilingManager)
        {
            _time = time;
            _entityManager = entityManager;
            _colliderSystem = colliderSystem;
            _profilingManager = profilingManager;
        }
        
        public void InitializeController()
        {
            _raycasterQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(RaycastComponent)
            });
            _entityBuffer = _arrayUtil.CreatePersistentArray<Entity>(EntityBufferChunkSize);
            _entityCount = _arrayUtil.CreatePersistentArray<int>(1);
            _entityCount[0] = 0;
            _profiler = _profilingManager.GetHandle(this);
        }

        public void UpdateController()
        {
            _profiler.BeginSample("Query Chunks");
            var raycasterChunks = _raycasterQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query Chunks");

            _profiler.BeginSample("Compute Offsets");
            var raycasterChunkCount = raycasterChunks.Length;
            var raycasterOffsets = _arrayUtil.CreateTempJobArray<int>(raycasterChunkCount);
            var raycasterCount = 0;
            for (var i = 0; i < raycasterChunkCount; i++)
            {
                raycasterOffsets[i] = raycasterCount;
                raycasterCount += raycasterChunks[i].Count;
            }
            _profiler.EndSample("Compute Offsets");

            _profiler.BeginSample("Raycast");
            var raycastResultCounts = _arrayUtil.CreateTempJobArray<int>(raycasterChunkCount);
            _arrayUtil.MaintainPersistentArrayLength(ref _entityBuffer, raycasterCount, EntityBufferChunkSize);
            var raycastJob = new RaycastJob
            {
                raycasterChunks = raycasterChunks,
                resultWriteOffsets = raycasterOffsets,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                inColliderWorld = _colliderSystem.ColliderWorld,
                deltaTime = _time.DeltaTime,
                resultCounts = raycastResultCounts,
                resultEntities = _entityBuffer
            };
            var raycastHandle = raycastJob.Schedule(raycasterChunkCount, 1);
            raycastHandle.Complete();
            _profiler.EndSample("Raycast");
            
            _profiler.BeginSample("Collect Results");
            var collectJob = new SingleBufferedDataCollectJob<Entity>
            {
                inOutData = _entityBuffer,
                inOffsets = raycasterOffsets, 
                inCounts = raycastResultCounts,
                outCount = _entityCount,
            };
            var collectHandle = collectJob.Schedule(raycastHandle);
            collectHandle.Complete();
            _profiler.EndSample("Collect Results");
            
            _profiler.BeginSample("Dispose arrays");
            raycasterChunks.Dispose();
            raycasterOffsets.Dispose();
            raycastResultCounts.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void FinalizeController()
        {
            _entityBuffer.Dispose();
            _entityCount.Dispose();
        }
    }
}