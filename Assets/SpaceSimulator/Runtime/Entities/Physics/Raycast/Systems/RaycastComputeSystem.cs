using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Extensions;
using SpaceSimulator.Runtime.Entities.Physics.Collision;
using SpaceSimulator.Runtime.Entities.Physics.Velocity;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Physics.Raycast
{
    public class RaycastComputeSystem : SystemBase
    {
        private const int EntityBufferChunkSize = 4096;

        public NativeArray<Entity> HitEntities => _entityBuffer;
        public int HitCount => _entityCount[0];
        
        private EntityQuery _colliderQuery;
        private EntityQuery _raycasterQuery;
        private NativeArray<Entity> _entityBuffer;
        private NativeArray<int> _entityCount;
        private SystemBaseUtil _util;
        
        protected override void OnCreate()
        {
            _colliderQuery = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _raycasterQuery = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(RaycastComponent)
            });
            _entityBuffer = _util.CreatePersistentArray<Entity>(EntityBufferChunkSize);
            _entityCount = _util.CreatePersistentArray<int>(1);
            _entityCount[0] = 0;
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("_colliderQuery.CreateArchetypeChunkArray");
            var colliderChunks = _colliderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();
            
            Profiler.BeginSample("colliderOffsets");
            var colliderChunkCount = colliderChunks.Length;
            var colliderOffsets = _util.CreateTempJobArray<int>(colliderChunkCount);
            var colliderCount = 0;
            for (var i = 0; i < colliderChunkCount; i++)
            {
                colliderOffsets[i] = colliderCount;
                colliderCount += colliderChunks[i].Count;
            }
            var colliderBounds = _util.CreateTempJobArray<BakedColliderData>(colliderCount);
            Profiler.EndSample();
            
            Profiler.BeginSample("BakeCollidersJob");
            var bakeCollidersJob = new BakeCollidersJob
            {
                colliderChunks = colliderChunks,
                boundsWriteOffsets = colliderOffsets,
                outputBounds = colliderBounds,
                colliderHandle = GetComponentTypeHandle<ColliderComponent>(true),
                positionHandle = GetComponentTypeHandle<PositionComponent>(true)
            };
            var bakeHandle = bakeCollidersJob.Schedule(colliderChunkCount, 16, Dependency);
            bakeHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("_raycasterQuery.CreateArchetypeChunkArray");
            var raycasterChunks = _raycasterQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("raycasterOffsets");
            var raycasterChunkCount = raycasterChunks.Length;
            var raycasterOffsets = _util.CreateTempJobArray<int>(raycasterChunkCount);
            var raycasterCount = 0;
            for (var i = 0; i < raycasterChunkCount; i++)
            {
                raycasterOffsets[i] = raycasterCount;
                raycasterCount += raycasterChunks[i].Count;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Raycast and collect results");
            var raycastResultCounts = _util.CreateTempJobArray<int>(raycasterChunkCount);
            _util.MaintainPersistentArrayLength(ref _entityBuffer, raycasterCount, EntityBufferChunkSize);
            var raycastJob = new RaycastJob
            {
                raycasterChunks = raycasterChunks,
                resultWriteOffsets = raycasterOffsets,
                positionHandle = GetComponentTypeHandle<PositionComponent>(),
                velocityHandle = GetComponentTypeHandle<VelocityComponent>(),
                entityHandle = GetEntityTypeHandle(),
                colliders = colliderBounds,
                deltaTime = Time.DeltaTime,
                resultCounts = raycastResultCounts,
                resultEntities = _entityBuffer
            };
            var raycastHandle = raycastJob.Schedule(raycasterChunkCount, 1, bakeHandle);

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
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            colliderBounds.Dispose();
            raycasterChunks.Dispose();
            raycasterOffsets.Dispose();
            raycastResultCounts.Dispose();
            Profiler.EndSample();
            
            SpaceDebug.LogState("ColliderCount", colliderCount);
        }

        protected override void OnDestroy()
        {
            _entityBuffer.Dispose();
            _entityCount.Dispose();
        }
    }
}