using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal partial class ColliderBakeSystem : IController, IColliderBakeSystem
    {
        public EControllerType ControllerType => EControllerType.EntityCompute;
        
        public ColliderWorld ColliderWorld { get; private set; }
        
        private const int ColliderBufferChunkSize = 512;
        private const int ChunkBufferChunkSize = 256;
        private const int MappingJobCount = 8;
        private const int MaxCellCount = 65536;

        private readonly IEntityWorldManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private GridUtil _gridUtil;
        private DebugUtil _debugUtil;
        private NativeArray<FloatBounds> _colliderBounds;
        private NativeArray<ushort> _worldColliders;
        private NativeArray<ColliderListPointer> _worldChunks;
        private ProfilingHandle _profiler;

        public ColliderBakeSystem(IEntityWorldManager entityManager, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
        }
        
        public void InitializeController()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = NativeMemoryUtil.CreatePersistentArray<FloatBounds>(ColliderBufferChunkSize);
            _worldColliders = NativeMemoryUtil.CreatePersistentArray<ushort>(ColliderBufferChunkSize * 4);
            _worldChunks = NativeMemoryUtil.CreatePersistentArray<ColliderListPointer>(ChunkBufferChunkSize);
        }

        public void UpdateController()
        {
            _profiler.BeginSample("Query Chunks");
            var colliderChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query Chunks");

            _profiler.BeginSample("Collider Offsets");
            var colliderChunkCount = colliderChunks.Length;
            var colliderOffsets = NativeMemoryUtil.CreateTempJobArray<int>(colliderChunkCount);
            var colliderCount = 0;
            for (var i = 0; i < colliderChunkCount; i++)
            {
                colliderOffsets[i] = colliderCount;
                colliderCount += colliderChunks[i].Count;
            }
            _profiler.EndSample("Collider Offsets");
            
            _profiler.BeginSample("Colliders Bounds");
            NativeMemoryUtil.MaintainPersistentArrayLength(ref _colliderBounds, colliderCount, ColliderBufferChunkSize);
            var computeBoundsJob = new ComputeBoundsJob
            {
                colliderChunks = colliderChunks,
                boundsWriteOffsets = colliderOffsets,
                outputBounds = _colliderBounds,
                colliderHandle = _entityManager.GetComponentTypeHandle<ColliderComponent>(true),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true)
            };
            var computeBoundsJobHandle = computeBoundsJob.Schedule(colliderChunkCount, 8);
            computeBoundsJobHandle.Complete();
            _profiler.EndSample("Colliders Bounds");
            
            _profiler.BeginSample("Grid");
            var worldGrid = _gridUtil.ComputeGrid(_colliderBounds, colliderCount, _profiler);
            _profiler.EndSample("Grid");
            
            _profiler.BeginSample("Reset Chunks");
            var worldChunkTotal = worldGrid.size.x * worldGrid.size.y;
            
            NativeMemoryUtil.MaintainPersistentArrayLength(ref _worldChunks, worldChunkTotal, ChunkBufferChunkSize);
            
            var resetChunksJob = new FillNativeArrayJob<ColliderListPointer>
            {
                inItemPerJob = 128,
                inValue = default,
                inTotalItem = worldChunkTotal,
                outNativeArray = _worldChunks
            };
            var jobCount = (int) math.ceil(worldChunkTotal / 128f);
            var resetChunksJobHandle = resetChunksJob.Schedule(jobCount, 8);
            resetChunksJobHandle.Complete();
            _profiler.EndSample("Reset Chunks");

            _profiler.BeginSample("Chunk Colliders");
            jobCount = (int) math.ceil(colliderCount / 128f);
            var chunkedColliders = NativeMemoryUtil.CreateTempJobArray<ChunkedCollider>(colliderCount * 4);
            var chunkedColliderCounts = NativeMemoryUtil.CreateTempJobArray<int>(jobCount);
            
            var chunkingJob = new ChunkCollidersJob
            {
                inColliderBounds = _colliderBounds,
                inWorldGrid = worldGrid,
                inColliderPerJob = 128,
                inColliderTotalCount = colliderCount,
                outColliders = chunkedColliders,
                outColliderCount = chunkedColliderCounts
            };
            var chunkingJobHandle = chunkingJob.Schedule(jobCount, 8);
            chunkingJobHandle.Complete();
            _profiler.EndSample("Chunk Colliders");
            
            _profiler.BeginSample("Lists Capacity");
            var listCapacityJob = new WorldChunkListsCapacityJob
            {
                inColliders = chunkedColliders,
                inColliderBatchCapacity = 128 * 4,
                inColliderCounts = chunkedColliderCounts,
                inOutLists = _worldChunks
            };
            var listCapacityJobHandle = listCapacityJob.Schedule();
            listCapacityJobHandle.Complete();
            _profiler.EndSample("Lists Capacity");
            
            _profiler.BeginSample("Lists Offsets");
            var listOffsetsJob = new WorldChunkListsOffsetJob
            {
                inListCount = worldChunkTotal,
                inOutLists = _worldChunks
            };
            var listOffsetsJobHandle = listOffsetsJob.Schedule();
            listOffsetsJobHandle.Complete();
            _profiler.EndSample("Lists Offsets");
            
            _profiler.BeginSample("Lists Fill");
            NativeMemoryUtil.MaintainPersistentArrayLength(ref _worldColliders, colliderCount * 4, ChunkBufferChunkSize * 4);
            var listFillJob = new WorldChunkListsFillJob
            {
                inColliders = chunkedColliders,
                inColliderBatchCapacity = 128 * 4,
                inColliderCounts = chunkedColliderCounts,
                inOutLists = _worldChunks,
                outColliders = _worldColliders
            };
            var listFillJobHandle = listFillJob.Schedule();
            listFillJobHandle.Complete();
            _profiler.EndSample("Lists Fill");

            _profiler.BeginSample("Dispose Arrays");
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            chunkedColliderCounts.Dispose();
            chunkedColliders.Dispose();
            _profiler.EndSample("Dispose Arrays");

            ColliderWorld = new ColliderWorld
            {
                colliders = new NativeSlice<FloatBounds>(_colliderBounds, 0, colliderCount),
                colliderStream = new NativeSlice<ushort>(_worldColliders, 0, _worldColliders.Length),
                worldCells = new NativeSlice<ColliderListPointer>(_worldChunks, 0, _worldChunks.Length),
                worldGrid = worldGrid
            };

            SpaceDebug.LogState("ColliderCount", colliderCount);
            _debugUtil.LogWorld(worldGrid);
        }

        public void FinalizeController()
        {
            _colliderBounds.Dispose();
            _worldColliders.Dispose();
            _worldChunks.Dispose();
        }
    }
}