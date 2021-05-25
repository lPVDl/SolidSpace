using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal partial class ColliderBakeSystem : IController, IColliderBakeSystem
    {
        public EControllerType ControllerType => EControllerType.EntityCompute;
        
        public ColliderWorld ColliderWorld { get; private set; }
        
        private const int ColliderBufferChunkSize = 512;
        private const int ChunkBufferChunkSize = 256;
        private const int MaxCellCount = 65536;

        private readonly IEntityWorldManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly IGizmosManager _gizmosManager;
        private readonly ColliderBakeSystemConfig _config;

        private EntityQuery _query;
        private GridUtil _gridUtil;
        private NativeArray<FloatBounds> _colliderBounds;
        private NativeArray<ColliderShape> _colliderShapes;
        private NativeArray<ushort> _worldColliders;
        private NativeArray<ColliderListPointer> _worldChunks;
        private ProfilingHandle _profiler;
        private GizmosHandle _gizmos;

        public ColliderBakeSystem(IEntityWorldManager entityManager, IProfilingManager profilingManager,
            IGizmosManager gizmosManager, ColliderBakeSystemConfig config)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void InitializeController()
        {
            _profiler = _profilingManager.GetHandle(this);
            _gizmos = _gizmosManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = NativeMemoryUtil.CreatePersistentArray<FloatBounds>(ColliderBufferChunkSize);
            _colliderShapes = NativeMemoryUtil.CreatePersistentArray<ColliderShape>(ColliderBufferChunkSize);
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
            NativeMemoryUtil.MaintainPersistentArrayLength(ref _colliderShapes, colliderCount, ColliderBufferChunkSize);
            var computeBoundsJob = new ComputeBoundsJob
            {
                inChunks = colliderChunks,
                inWriteOffsets = colliderOffsets,
                outBounds = _colliderBounds,
                outShapes = _colliderShapes,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                sizeHandle = _entityManager.GetComponentTypeHandle<SizeComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(true)
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
            
            // TODO [T-24]: Collider fill list job should be parallel.
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
                colliderBounds = new NativeSlice<FloatBounds>(_colliderBounds, 0, colliderCount),
                colliderShapes = new NativeSlice<ColliderShape>(_colliderShapes, 0, colliderCount),
                colliderStream = new NativeSlice<ushort>(_worldColliders, 0, _worldColliders.Length),
                worldCells = new NativeSlice<ColliderListPointer>(_worldChunks, 0, _worldChunks.Length),
                worldGrid = worldGrid
            };
            
            SpaceDebug.LogState("ColliderCount", colliderCount);
            
            LogWorld(worldGrid);
        }
        
        private void LogWorld(ColliderWorldGrid worldGrid)
        {
            var cellSize = 1 << worldGrid.power;
            var cellCountX = worldGrid.size.x;
            var cellCountY = worldGrid.size.y;
            var worldMin = worldGrid.anchor * cellSize;
            var worldMax = (worldGrid.anchor + worldGrid.size) * cellSize;
            
            SpaceDebug.LogState("ColliderCellCountX", cellCountX);
            SpaceDebug.LogState("ColliderCellCountY", cellCountY);
            SpaceDebug.LogState("ColliderCellSize", cellSize);

            var gridColor = _config.GridColor;
            
            _gizmos.DrawLine(worldMin.x, worldMin.y, worldMin.x, worldMax.y, gridColor);
            _gizmos.DrawLine(worldMin.x, worldMax.y, worldMax.x, worldMax.y, gridColor);
            _gizmos.DrawLine(worldMax.x, worldMax.y, worldMax.x, worldMin.y, gridColor);
            _gizmos.DrawLine(worldMax.x, worldMin.y, worldMin.x, worldMin.y, gridColor);

            for (var i = 1; i < cellCountX; i++)
            {
                var p0 = new Vector2(worldMin.x + cellSize * i, worldMax.y);
                var p1 = new Vector2(worldMin.x + cellSize * i, worldMin.y);
                _gizmos.DrawLine(p0, p1, gridColor);
            }
            
            for (var i = 1; i < cellCountY; i++)
            {
                var p2 = new Vector2(worldMin.x, worldMin.y + i * cellSize);
                var p3 = new Vector2(worldMax.x, worldMin.y + i * cellSize);
                _gizmos.DrawLine(p2, p3, gridColor);
            }
        }

        public void FinalizeController()
        {
            _colliderBounds.Dispose();
            _colliderShapes.Dispose();
            _worldColliders.Dispose();
            _worldChunks.Dispose();
        }
    }
}