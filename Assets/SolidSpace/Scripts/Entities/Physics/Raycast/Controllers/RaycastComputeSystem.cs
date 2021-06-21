using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Raycast
{
    internal class RaycastComputeSystem : IInitializable, IUpdatable, IRaycastSystem
    {
        private const int EntityPerAllocation = 4096;
        
        public RaycastWorld World { get; private set; }

        private readonly IEntityWorldManager _entityManager;
        private readonly IColliderSystem _colliderSystem;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _raycasterQuery;
        private ProfilingHandle _profiler;
        
        private NativeArray<Entity> _hitEntities;
        private NativeArray<ushort> _hitColliderIndices;
        private NativeArray<byte> _hitEntityArchetypeIndices;
        private NativeArray<FloatRay> _hitRayOrigins;
        private NativeArray<EntityArchetype> _hitEntityArchetypes;
        private NativeReference<int> _hitCount;

        public RaycastComputeSystem(IEntityWorldManager entityManager, IColliderSystem colliderSystem,
            IEntityWorldTime time, IProfilingManager profilingManager)
        {
            _time = time;
            _entityManager = entityManager;
            _colliderSystem = colliderSystem;
            _profilingManager = profilingManager;
        }
        
        public void OnInitialize()
        {
            _raycasterQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(RaycastComponent)
            });
            _hitEntities = NativeMemory.CreatePersistentArray<Entity>(EntityPerAllocation);
            _hitColliderIndices = NativeMemory.CreatePersistentArray<ushort>(EntityPerAllocation);
            _hitEntityArchetypeIndices = NativeMemory.CreatePersistentArray<byte>(EntityPerAllocation);
            _hitRayOrigins = NativeMemory.CreatePersistentArray<FloatRay>(EntityPerAllocation);
            _hitCount = NativeMemory.CreatePersistentReference(0);
            _hitEntityArchetypes = NativeMemory.CreatePersistentArray<EntityArchetype>(256);
            _profiler = _profilingManager.GetHandle(this);
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query Chunks");
            var raycasterChunks = _raycasterQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query Chunks");

            _profiler.BeginSample("Compute Offsets & Archetypes");
            var raycasterChunkCount = raycasterChunks.Length;
            var raycasterOffsets = NativeMemory.CreateTempJobArray<int>(raycasterChunkCount);
            var chunkArchetypeIndices = NativeMemory.CreateTempJobArray<byte>(raycasterChunkCount);
            var archetypeCount = 0;
            var raycasterCount = 0;
            for (var i = 0; i < raycasterChunkCount; i++)
            {
                var chunk = raycasterChunks[i];
                var archetype = chunk.Archetype;
                var archetypeFound = false;
                for (var j = 0; j < archetypeCount; j++)
                {
                    if (archetype == _hitEntityArchetypes[j])
                    {
                        chunkArchetypeIndices[i] = (byte) j;
                        archetypeFound = true;
                        break;
                    }
                }
                if (!archetypeFound)
                {
                    _hitEntityArchetypes[archetypeCount] = archetype;
                    chunkArchetypeIndices[i] = (byte) archetypeCount++;
                }
                
                raycasterOffsets[i] = raycasterCount;
                raycasterCount += chunk.Count;
            }
            _profiler.EndSample("Compute Offsets & Archetypes");

            _profiler.BeginSample("Raycast");
            var raycastResultCounts = NativeMemory.CreateTempJobArray<int>(raycasterChunkCount);
            var maintenanceRule = new ArrayMaintenanceData
            {
                requiredCapacity = raycasterCount,
                itemPerAllocation = EntityPerAllocation,
                copyOnResize = false
            };
            NativeMemory.MaintainPersistentArrayLength(ref _hitEntities, maintenanceRule);
            NativeMemory.MaintainPersistentArrayLength(ref _hitColliderIndices, maintenanceRule);
            NativeMemory.MaintainPersistentArrayLength(ref _hitEntityArchetypeIndices, maintenanceRule);
            NativeMemory.MaintainPersistentArrayLength(ref _hitRayOrigins, maintenanceRule);

            new RaycastJob
            {
                inRaycasterChunks = raycasterChunks,
                inResultWriteOffsets = raycasterOffsets,
                inRaycasterArchetypeIndices = chunkArchetypeIndices,
                inColliderWorld = _colliderSystem.World,
                inDeltaTime = _time.DeltaTime,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                outCounts = raycastResultCounts,
                outRaycasterEntities = _hitEntities,
                outColliderIndices = _hitColliderIndices,
                outRaycasterArchetypeIndices = _hitEntityArchetypeIndices,
                outRaycastOrigins = _hitRayOrigins
            }.Schedule(raycasterChunkCount, 1).Complete();
            _profiler.EndSample("Raycast");
            
            _profiler.BeginSample("Collect Results");
            new DataCollectJobWithOffsets<Entity, byte, ushort, FloatRay>
            {
                inOutData0 = _hitEntities,
                inOutData1 = _hitEntityArchetypeIndices,
                inOutData2 = _hitColliderIndices,
                inOutData3 = _hitRayOrigins,
                inOffsets = raycasterOffsets, 
                inCounts = raycastResultCounts,
                outCount = _hitCount,
            }.Schedule().Complete();
            _profiler.EndSample("Collect Results");
            
            _profiler.BeginSample("Dispose Arrays");
            raycasterOffsets.Dispose();
            raycastResultCounts.Dispose();
            chunkArchetypeIndices.Dispose();
            raycasterChunks.Dispose();
            _profiler.EndSample("Dispose Arrays");

            World = new RaycastWorld
            {
                raycastArchetypes = new NativeSlice<EntityArchetype>(_hitEntityArchetypes, 0, archetypeCount),
                raycastEntities = new NativeSlice<Entity>(_hitEntities, 0, _hitCount.Value),
                raycastArchetypeIndices = new NativeSlice<byte>(_hitEntityArchetypeIndices, 0, _hitCount.Value),
                raycastOrigins = new NativeSlice<FloatRay>(_hitRayOrigins, 0, _hitCount.Value),
                colliderIndices = new NativeSlice<ushort>(_hitColliderIndices, 0, _hitCount.Value)
            };
        }

        public void OnFinalize()
        {
            _hitRayOrigins.Dispose();
            _hitEntityArchetypeIndices.Dispose();
            _hitEntityArchetypes.Dispose();
            _hitColliderIndices.Dispose();
            _hitEntities.Dispose();
            _hitCount.Dispose();
        }
    }
}