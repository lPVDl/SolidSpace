using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace SpaceSimulator.Entities.Physics
{
    public class RaycastComputeSystem : IEntitySystem, IRaycastComputeSystem
    {
        private const int EntityBufferChunkSize = 4096;

        public ESystemType SystemType => ESystemType.Compute;
        
        public NativeArray<Entity> HitEntities => _entityBuffer;
        public int HitCount => _entityCount[0];

        private readonly IEntityManager _entityManager;
        private readonly IColliderBakeSystem _colliderSystem;
        private readonly IEntityWorldTime _time;

        private EntityQuery _raycasterQuery;
        private NativeArray<Entity> _entityBuffer;
        private NativeArray<int> _entityCount;
        private NativeArrayUtil _arrayUtil;

        public RaycastComputeSystem(IEntityManager entityManager, IColliderBakeSystem colliderSystem, IEntityWorldTime time)
        {
            _entityManager = entityManager;
            _colliderSystem = colliderSystem;
            _time = time;
        }
        
        public void Initialize()
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
        }

        public void Update()
        {
            Profiler.BeginSample("_raycasterQuery.CreateArchetypeChunkArray");
            var raycasterChunks = _raycasterQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("Raycaster offsets");
            var raycasterChunkCount = raycasterChunks.Length;
            var raycasterOffsets = _arrayUtil.CreateTempJobArray<int>(raycasterChunkCount);
            var raycasterCount = 0;
            for (var i = 0; i < raycasterChunkCount; i++)
            {
                raycasterOffsets[i] = raycasterCount;
                raycasterCount += raycasterChunks[i].Count;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Raycast");
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
            Profiler.EndSample();
            
            Profiler.BeginSample("Collect results");
            var collectJob = new SingleBufferedDataCollectJob<Entity>
            {
                inOutData = _entityBuffer,
                inOffsets = raycasterOffsets, 
                inCounts = raycastResultCounts,
                outCount = _entityCount,
            };
            var collectHandle = collectJob.Schedule(raycastHandle);
            collectHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Dispose arrays");
            
            raycasterChunks.Dispose();
            raycasterOffsets.Dispose();
            raycastResultCounts.Dispose();
            
            Profiler.EndSample();
        }

        public void FinalizeSystem()
        {
            _entityBuffer.Dispose();
            _entityCount.Dispose();
        }
    }
}