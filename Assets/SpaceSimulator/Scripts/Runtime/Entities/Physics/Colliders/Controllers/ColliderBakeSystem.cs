using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public partial class ColliderBakeSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Compute;
        
        public ColliderWorld ColliderWorld { get; private set; }
        
        private const int ColliderBufferChunkSize = 512;
        private const int ChunkBufferChunkSize = 256;
        private const int MappingJobCount = 8;
        private const int MaxCellCount = 65536;

        private readonly World _world;
        
        private EntityQuery _query;
        private EntitySystemUtil _systemUtil;
        private GridUtil _gridUtil;
        private DebugUtil _debugUtil;
        private NativeArray<FloatBounds> _colliderBounds;
        private NativeArray<ushort> _worldColliders;
        private NativeArray<ColliderListPointer> _worldChunks;

        public ColliderBakeSystem(World world)
        {
            _world = world;
        }
        
        public void Initialize()
        {
            _query = _world.EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = _systemUtil.CreatePersistentArray<FloatBounds>(ColliderBufferChunkSize);
            _worldColliders = _systemUtil.CreatePersistentArray<ushort>(ColliderBufferChunkSize * 4);
            _worldChunks = _systemUtil.CreatePersistentArray<ColliderListPointer>(ChunkBufferChunkSize);
        }

        public void Update()
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
            
            Profiler.BeginSample("Compute Colliders Bounds");
            _systemUtil.MaintainPersistentArrayLength(ref _colliderBounds, colliderCount, ColliderBufferChunkSize);
            var computeBoundsJob = new ComputeBoundsJob
            {
                colliderChunks = colliderChunks,
                boundsWriteOffsets = colliderOffsets,
                outputBounds = _colliderBounds,
                colliderHandle = _world.EntityManager.GetComponentTypeHandle<ColliderComponent>(true),
                positionHandle = _world.EntityManager.GetComponentTypeHandle<PositionComponent>(true)
            };
            var computeBoundsJobHandle = computeBoundsJob.Schedule(colliderChunkCount, 8);
            computeBoundsJobHandle.Complete();
            Profiler.EndSample();

            var worldGrid = _gridUtil.ComputeGrid(_colliderBounds, colliderCount);
            
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
            var resetChunksJobHandle = resetChunksJob.Schedule(jobCount, 8);
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
            var chunkingJobHandle = chunkingJob.Schedule(jobCount, 8);
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
                colliders = new NativeSlice<FloatBounds>(_colliderBounds, 0, colliderCount),
                colliderStream = new NativeSlice<ushort>(_worldColliders, 0, _worldColliders.Length),
                worldCells = new NativeSlice<ColliderListPointer>(_worldChunks, 0, _worldChunks.Length),
                worldGrid = worldGrid
            };

            SpaceDebug.LogState("ColliderCount", colliderCount);
            _debugUtil.LogWorld(worldGrid);
        }

        public void FinalizeSystem()
        {
            _colliderBounds.Dispose();
            _worldColliders.Dispose();
            _worldChunks.Dispose();
        }
    }
}