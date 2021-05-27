using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Raycast
{
    internal class RaycastComputeSystem : IController, IRaycastComputeSystem
    {
        private const int EntityPerAllocation = 4096;

        public EControllerType ControllerType => EControllerType.EntityCompute;
        
        public RaycastWorldData RaycastWorld { get; private set; }

        private readonly IEntityWorldManager _entityManager;
        private readonly IColliderBakeSystem _colliderSystem;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _raycasterQuery;
        private NativeArray<RaycastHit> _hitsBuffer;
        private NativeReference<int> _hitCount;
        private NativeArray<EntityArchetype> _archetypes;
        private ProfilingHandle _profiler;

        public RaycastComputeSystem(IEntityWorldManager entityManager, IColliderBakeSystem colliderSystem,
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
            _hitsBuffer = NativeMemory.CreatePersistentArray<RaycastHit>(EntityPerAllocation);
            _hitCount = NativeMemory.CreatePersistentReference(0);
            _archetypes = NativeMemory.CreatePersistentArray<EntityArchetype>(256);
            _profiler = _profilingManager.GetHandle(this);
        }

        public void UpdateController()
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
                    if (archetype == _archetypes[j])
                    {
                        chunkArchetypeIndices[i] = (byte) j;
                        archetypeFound = true;
                        break;
                    }
                }
                if (!archetypeFound)
                {
                    _archetypes[archetypeCount] = archetype;
                    chunkArchetypeIndices[i] = (byte) archetypeCount++;
                }
                
                raycasterOffsets[i] = raycasterCount;
                raycasterCount += chunk.Count;
            }
            _profiler.EndSample("Compute Offsets & Archetypes");

            _profiler.BeginSample("Raycast");
            var raycastResultCounts = NativeMemory.CreateTempJobArray<int>(raycasterChunkCount);
            NativeMemory.MaintainPersistentArrayLength(ref _hitsBuffer, new ArrayMaintenanceData
            {
                requiredCapacity = raycasterCount,
                itemPerAllocation = EntityPerAllocation,
                copyOnResize = false
            });

            var raycastJob = new RaycastJob
            {
                inRaycasterChunks = raycasterChunks,
                inResultWriteOffsets = raycasterOffsets,
                inRaycasterArchetypes = chunkArchetypeIndices,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                inColliderWorld = _colliderSystem.ColliderWorld,
                inDeltaTime = _time.DeltaTime,
                outCounts = raycastResultCounts,
                outHits = _hitsBuffer
            };
            var raycastHandle = raycastJob.Schedule(raycasterChunkCount, 1);
            raycastHandle.Complete();
            _profiler.EndSample("Raycast");
            
            _profiler.BeginSample("Collect Results");
            var collectJob = new SingleBufferedDataCollectJob<RaycastHit>
            {
                inOutData = _hitsBuffer,
                inOffsets = raycasterOffsets, 
                inCounts = raycastResultCounts,
                outCount = _hitCount,
            };
            var collectHandle = collectJob.Schedule(raycastHandle);
            collectHandle.Complete();
            _profiler.EndSample("Collect Results");
            
            _profiler.BeginSample("Dispose arrays");
            raycasterChunks.Dispose();
            raycasterOffsets.Dispose();
            raycastResultCounts.Dispose();
            chunkArchetypeIndices.Dispose();
            _profiler.EndSample("Dispose arrays");

            RaycastWorld = new RaycastWorldData
            {
                archetypes = new NativeSlice<EntityArchetype>(_archetypes, 0, archetypeCount),
                hits = new NativeSlice<RaycastHit>(_hitsBuffer, 0, _hitCount.Value)
            };
        }

        public void FinalizeController()
        {
            _hitsBuffer.Dispose();
            _hitCount.Dispose();
        }
    }
}