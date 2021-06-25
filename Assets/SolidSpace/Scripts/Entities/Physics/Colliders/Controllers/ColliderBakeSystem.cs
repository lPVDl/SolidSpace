using System;
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

namespace SolidSpace.Entities.Physics.Colliders
{
    internal partial class ColliderBakeSystem : IInitializable, IUpdatable, IColliderSystem
    {
        public ColliderWorld World { get; private set; }

        private const int ColliderPerAllocation = 512;
        private const int ChunkPerAllocation = 256;
        private const int MaxCellCount = 65536;

        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private GridUtil _gridUtil;
        private NativeArray<Entity> _colliderEntities;
        private NativeArray<FloatBounds> _colliderBounds;
        private NativeArray<ColliderShape> _colliderShapes;
        private NativeArray<EntityArchetype> _colliderArchetypes;
        private NativeArray<byte> _colliderArchetypeIndices;
        private NativeArray<ushort> _worldColliders;
        private NativeArray<ColliderListPointer> _worldChunks;
        private ProfilingHandle _profiler;

        public ColliderBakeSystem(IEntityManager entityManager, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = NativeMemory.CreatePersistentArray<FloatBounds>(ColliderPerAllocation);
            _colliderShapes = NativeMemory.CreatePersistentArray<ColliderShape>(ColliderPerAllocation);
            _colliderEntities = NativeMemory.CreatePersistentArray<Entity>(ColliderPerAllocation);
            _worldColliders = NativeMemory.CreatePersistentArray<ushort>(ColliderPerAllocation * 4);
            _worldChunks = NativeMemory.CreatePersistentArray<ColliderListPointer>(ChunkPerAllocation);
            _colliderArchetypes = NativeMemory.CreatePersistentArray<EntityArchetype>(256);
            _colliderArchetypeIndices = NativeMemory.CreatePersistentArray<byte>(ColliderPerAllocation);
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query Chunks");
            var colliderChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query Chunks");
            
            _profiler.BeginSample("Collider Offsets & Archetypes");
            var colliderChunkCount = colliderChunks.Length;
            var colliderOffsets = NativeMemory.CreateTempJobArray<int>(colliderChunkCount);
            var chunkArchetypeIndices = NativeMemory.CreateTempJobArray<byte>(colliderChunkCount);
            var archetypeCount = 0;
            var colliderCount = 0;
            for (var i = 0; i < colliderChunkCount; i++)
            {
                var chunk = colliderChunks[i];
                var archetype = chunk.Archetype;
                var archetypeFound = false;
                for (var j = 0; j < archetypeCount; j++)
                {
                    if (archetype == _colliderArchetypes[j])
                    {
                        chunkArchetypeIndices[i] = (byte) j;
                        archetypeFound = true;
                        break;
                    }
                }

                if (!archetypeFound)
                {
                    _colliderArchetypes[archetypeCount] = archetype;
                    chunkArchetypeIndices[i] = (byte) archetypeCount++;
                }
                
                colliderOffsets[i] = colliderCount;
                colliderCount += chunk.Count;
            }
            _profiler.EndSample("Collider Offsets & Archetypes");
            
            _profiler.BeginSample("Colliders Bounds");
            var arrayMaintenance = new ArrayMaintenanceData
            {
                requiredCapacity = colliderCount,
                itemPerAllocation = ColliderPerAllocation,
                copyOnResize = false
            };
            NativeMemory.MaintainPersistentArrayLength(ref _colliderBounds, arrayMaintenance);
            NativeMemory.MaintainPersistentArrayLength(ref _colliderShapes, arrayMaintenance);
            NativeMemory.MaintainPersistentArrayLength(ref _colliderEntities, arrayMaintenance);
            NativeMemory.MaintainPersistentArrayLength(ref _colliderArchetypeIndices, arrayMaintenance);
            var computeBoundsJob = new ComputeBoundsJob
            {
                inChunks = colliderChunks,
                inWriteOffsets = colliderOffsets,
                inArchetypeIndices = chunkArchetypeIndices,
                outBounds = _colliderBounds,
                outShapes = _colliderShapes,
                outArchetypeIndices = _colliderArchetypeIndices,
                outEntities = _colliderEntities,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                sizeHandle = _entityManager.GetComponentTypeHandle<SizeComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle()
            };
            var computeBoundsJobHandle = computeBoundsJob.Schedule(colliderChunkCount, 8);
            computeBoundsJobHandle.Complete();
            _profiler.EndSample("Colliders Bounds");
            
            _profiler.BeginSample("Grid");
            var worldGrid = _gridUtil.ComputeGrid(_colliderBounds, colliderCount, _profiler);
            _profiler.EndSample("Grid");
            
            _profiler.BeginSample("Reset Chunks");
            var worldChunkTotal = worldGrid.size.x * worldGrid.size.y;
            
            NativeMemory.MaintainPersistentArrayLength(ref _worldChunks, new ArrayMaintenanceData
            {
                requiredCapacity = worldChunkTotal,
                itemPerAllocation = ChunkPerAllocation,
                copyOnResize = false
            });
            
            var resetChunksJob = new FillNativeArrayJob<ColliderListPointer>
            {
                inItemPerJob = 128,
                inValue = default,
                inTotalItem = worldChunkTotal,
                outNativeArray = _worldChunks
            };
            var jobCount = (int) Math.Ceiling(worldChunkTotal / 128f);
            var resetChunksJobHandle = resetChunksJob.Schedule(jobCount, 8);
            resetChunksJobHandle.Complete();
            _profiler.EndSample("Reset Chunks");

            _profiler.BeginSample("Chunk Colliders");
            jobCount = (int) Math.Ceiling(colliderCount / 128f);
            var chunkedColliders = NativeMemory.CreateTempJobArray<ChunkedCollider>(colliderCount * 4);
            var chunkedColliderCounts = NativeMemory.CreateTempJobArray<int>(jobCount);

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
            NativeMemory.MaintainPersistentArrayLength(ref _worldColliders, new ArrayMaintenanceData
            {
                requiredCapacity = colliderCount * 4,
                itemPerAllocation = ChunkPerAllocation * 4,
                copyOnResize = false
            });
            
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
            chunkArchetypeIndices.Dispose();
            _profiler.EndSample("Dispose Arrays");

            World = new ColliderWorld
            {
                archetypes = new NativeSlice<EntityArchetype>(_colliderArchetypes, 0, archetypeCount),
                colliderArchetypeIndices = new NativeSlice<byte>(_colliderArchetypeIndices, 0, colliderCount),
                colliderBounds = new NativeSlice<FloatBounds>(_colliderBounds, 0, colliderCount),
                colliderShapes = new NativeSlice<ColliderShape>(_colliderShapes, 0, colliderCount),
                colliderEntities = new NativeSlice<Entity>(_colliderEntities, 0, colliderCount),
                colliderStream = new NativeSlice<ushort>(_worldColliders, 0, _worldColliders.Length),
                worldCells = new NativeSlice<ColliderListPointer>(_worldChunks, 0, _worldChunks.Length),
                worldGrid = worldGrid
            };
            
            SpaceDebug.LogState("ColliderCount", colliderCount);
            SpaceDebug.LogState("ColliderCellCountX", worldGrid.size.x);
            SpaceDebug.LogState("ColliderCellCountY", worldGrid.size.y);
            SpaceDebug.LogState("ColliderCellSize", 1 << worldGrid.power);
        }

        public void OnFinalize()
        {
            _colliderEntities.Dispose();
            _colliderBounds.Dispose();
            _colliderShapes.Dispose();
            _worldColliders.Dispose();
            _worldChunks.Dispose();
            _colliderArchetypes.Dispose();
            _colliderArchetypeIndices.Dispose();
        }
    }
}