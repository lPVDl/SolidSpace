using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public partial class ColliderBakeSystem : SystemBase
    {
        public ColliderWorld ColliderWorld { get; private set; }
        
        private const int ColliderBufferChunkSize = 512;
        private const int ChunkBufferChunkSize = 256;
        private const int MappingJobCount = 8;
        private const int MaxCellCount = 65536;

        private EntityQuery _query;
        private SystemBaseUtil _systemUtil;
        private GridUtil _gridUtil;
        private NativeArray<ColliderBounds> _colliderBounds;
        private NativeArray<ushort> _worldColliders;
        private NativeArray<ColliderListPointer> _worldChunks;

        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = _systemUtil.CreatePersistentArray<ColliderBounds>(ColliderBufferChunkSize);
            _worldColliders = _systemUtil.CreatePersistentArray<ushort>(ColliderBufferChunkSize * 4);
            _worldChunks = _systemUtil.CreatePersistentArray<ColliderListPointer>(ChunkBufferChunkSize);
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("Query collider chunks");
            var colliderChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("Collider Offsets");
            var colliderChunkCount = colliderChunks.Length;
            var colliderOffsets = _systemUtil.CreateTempJobArray<int>(colliderChunkCount);
            var colliderCount = 0;
            for (var i = 0; i < colliderChunkCount; i++)
            {
                colliderOffsets[i] = colliderCount;
                colliderCount += colliderChunks[i].Count;
            }

            Profiler.EndSample();

            _systemUtil.MaintainPersistentArrayLength(ref _colliderBounds, colliderCount, ColliderBufferChunkSize);

            Profiler.BeginSample("Compute Colliders Bounds");
            var computeBoundsJob = new ComputeBoundsJob
            {
                colliderChunks = colliderChunks,
                boundsWriteOffsets = colliderOffsets,
                outputBounds = _colliderBounds,
                colliderHandle = GetComponentTypeHandle<ColliderComponent>(true),
                positionHandle = GetComponentTypeHandle<PositionComponent>(true)
            };
            var computeBoundsJobHandle = computeBoundsJob.Schedule(colliderChunkCount, 8, Dependency);
            computeBoundsJobHandle.Complete();
            Profiler.EndSample();

            var worldGrid = _gridUtil.ComputeGrid(_colliderBounds, colliderCount);
            DebugDrawWorld(worldGrid);
            
            Profiler.BeginSample("Reset Chunks");
            var worldChunkTotal = worldGrid.size.x * worldGrid.size.y;
            
            _systemUtil.MaintainPersistentArrayLength(ref _worldChunks, worldChunkTotal, ChunkBufferChunkSize);
            
            var resetChunksJob = new FillNativeArrayJob<ColliderListPointer>
            {
                inItemPerJob = 128,
                inValue = default,
                inTotalItem = worldChunkTotal,
                outNativeArray = _worldChunks
            };
            var jobCount = (int) math.ceil(worldChunkTotal / 128f);
            var resetChunksJobHandle = resetChunksJob.Schedule(jobCount, 8, Dependency);
            resetChunksJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Chunk colliders");
            jobCount = (int) math.ceil(colliderCount / 128f);
            var chunkedColliders = _systemUtil.CreateTempJobArray<ChunkedCollider>(colliderCount * 4);
            var chunkedColliderCounts = _systemUtil.CreateTempJobArray<int>(jobCount);
            
            var chunkingJob = new ChunkCollidersJob
            {
                inColliderBounds = _colliderBounds,
                inWorldGrid = worldGrid,
                inColliderPerJob = 128,
                inColliderTotalCount = colliderCount,
                outColliders = chunkedColliders,
                outColliderCount = chunkedColliderCounts
            };
            var chunkingJobHandle = chunkingJob.Schedule(jobCount, 8, Dependency);
            chunkingJobHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Lists capacity");
            var listCapacityJob = new WorldChunkListsCapacityJob
            {
                inColliders = chunkedColliders,
                inColliderBatchCapacity = 128 * 4,
                inColliderCounts = chunkedColliderCounts,
                inOutLists = _worldChunks
            };
            var listCapacityJobHandle = listCapacityJob.Schedule();
            listCapacityJobHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Lists offsets");
            var listOffsetsJob = new WorldChunkListsOffsetJob
            {
                inListCount = worldChunkTotal,
                inOutLists = _worldChunks
            };
            var listOffsetsJobHandle = listOffsetsJob.Schedule();
            listOffsetsJobHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Lists fill");
            _systemUtil.MaintainPersistentArrayLength(ref _worldColliders, colliderCount * 4, ChunkBufferChunkSize * 4);
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
            Profiler.EndSample();

            Profiler.BeginSample("Dispose arrays");
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            chunkedColliderCounts.Dispose();
            chunkedColliders.Dispose();
            Profiler.EndSample();

            ColliderWorld = new ColliderWorld
            {
                colliders = new NativeSlice<ColliderBounds>(_colliderBounds, colliderCount)
            };

            SpaceDebug.LogState("ColliderCount", colliderCount);
        }

        private void DebugDrawWorld(ColliderWorldGrid worldGrid)
        {
            var cellSize = 1 << worldGrid.power;
            var cellCountX = worldGrid.size.x;
            var cellCountY = worldGrid.size.y;
            var worldMin = worldGrid.anchor * cellSize;
            var worldMax = (worldGrid.anchor + worldGrid.size) * cellSize;
            
            SpaceDebug.LogState("ColliderCellCountX", cellCountX);
            SpaceDebug.LogState("ColliderCellCountY", cellCountY);
            SpaceDebug.LogState("ColliderCellSize", cellSize);
            
            Debug.DrawLine(new Vector3(worldMin.x, worldMin.y), new Vector3(worldMin.x, worldMax.y));
            Debug.DrawLine(new Vector3(worldMin.x, worldMax.y), new Vector3(worldMax.x, worldMax.y));
            Debug.DrawLine(new Vector3(worldMax.x, worldMax.y), new Vector3(worldMax.x, worldMin.y));
            Debug.DrawLine(new Vector3(worldMax.x, worldMin.y), new Vector3(worldMin.x, worldMin.y));

            for (var i = 1; i < cellCountX; i++)
            {
                var p0 = new Vector3(worldMin.x + cellSize * i, worldMax.y, 0);
                var p1 = new Vector3(worldMin.x + cellSize * i, worldMin.y, 0);
                Debug.DrawLine(p0, p1);
            }
            
            for (var i = 1; i < cellCountY; i++)
            {
                var p2 = new Vector3(worldMin.x, worldMin.y + i * cellSize, 0);
                var p3 = new Vector3(worldMax.x, worldMin.y + i * cellSize, 0);
                Debug.DrawLine(p2, p3);
            }
        }

        protected override void OnDestroy()
        {
            _colliderBounds.Dispose();
            _worldColliders.Dispose();
            _worldChunks.Dispose();
        }
    }
}