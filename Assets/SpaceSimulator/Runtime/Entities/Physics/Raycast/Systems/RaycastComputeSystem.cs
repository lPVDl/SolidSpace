using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Extensions;
using SpaceSimulator.Runtime.Entities.Physics.Collision;
using SpaceSimulator.Runtime.Entities.Physics.Velocity;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Physics.Raycast
{
    public class RaycastComputeSystem : SystemBase
    {
        private const int EntityBufferChunkSize = 4096;
        private const float MinWorldSize = 1;
        private const float MinCellSize = 1;
        private const int MaxWorldCellCount = 128;

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
            
            Profiler.BeginSample("Collider Offsets");
            var colliderChunkCount = colliderChunks.Length;
            var colliderOffsets = _util.CreateTempJobArray<int>(colliderChunkCount);
            var colliderCount = 0;
            for (var i = 0; i < colliderChunkCount; i++)
            {
                colliderOffsets[i] = colliderCount;
                colliderCount += colliderChunks[i].Count;
            }
            var colliderBounds = _util.CreateTempJobArray<ColliderBounds>(colliderCount);
            Profiler.EndSample();
            
            Profiler.BeginSample("Compute Colliders Bounds");
            var bakeCollidersJob = new ComputeColliderBoundsJob
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
            
            Profiler.BeginSample("Compute World Bounds");
            var colliderJobCount = (int) (colliderCount / 128f + 0.5);
            var colliderJoinedBounds = _util.CreateTempJobArray<ColliderBounds>(colliderJobCount);
            var worldBoundsJob = new JoinBoundsJob
            {
                inBounds = colliderBounds,
                inBoundsPerJob = 128,
                outBounds = colliderJoinedBounds
            };
            var worldBoundsJobHandle = worldBoundsJob.Schedule(colliderJobCount, 1, Dependency);
            
            Profiler.BeginSample("Compute World Bounds : Jobs");
            worldBoundsJobHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Compute World Bounds : Main Thread");
            var worldBounds = JoinBounds(colliderJoinedBounds);
            Profiler.EndSample();

            var colliderMaxSizes = _util.CreateTempJobArray<float2>(colliderJobCount);
            var colliderSizesJob = new FindMaxColliderSizeJob
            {
                inBounds = colliderBounds,
                inBoundsPerJob = 128,
                outSizes = colliderMaxSizes
            };
            var colliderSizesJobHandle = colliderSizesJob.Schedule(colliderJobCount, 1, Dependency);
            
            Profiler.BeginSample("Find Max Collider Size : Jobs");
            colliderSizesJobHandle.Complete();
            Profiler.EndSample();
            
            Profiler.BeginSample("Find Max Collider Size : Main Thread");
            var maxColliderSizes = FindBoundsMaxSize(colliderMaxSizes);
            var worldCellSize = math.max(math.max(maxColliderSizes.x, maxColliderSizes.y), MinCellSize);
            Profiler.EndSample();
            
            var worldMin = new float2(worldBounds.xMin, worldBounds.yMin);
            var worldMax = new float2(worldBounds.xMax, worldBounds.yMax);
            var worldDelta = worldMax - worldMin;
            var worldSize = math.max(math.max(worldDelta.x, worldDelta.y), MinWorldSize);
            var cellCount = (int)(worldSize / worldCellSize + 0.5);
            cellCount = math.min(cellCount, MaxWorldCellCount);
            var cellSize = worldSize / cellCount;
            var worldCenter = (worldMax + worldMin) / 2;
            
            SpaceDebug.LogState("ColliderCellCount", cellCount);

            worldMin = worldCenter - worldSize / 2;
            worldMax = worldMin + worldSize;
            
            for (var i = 1; i < cellCount; i++)
            {
                var p0 = new Vector3(worldMin.x + cellSize * i, worldMax.y, 0);
                var p1 = new Vector3(worldMin.x + cellSize * i, worldMin.y, 0);
                var p2 = new Vector3(worldMin.x, worldMin.y + i * cellSize, 0);
                var p3 = new Vector3(worldMax.x, worldMin.y + i * cellSize, 0);
                Debug.DrawLine(p0, p1);
                Debug.DrawLine(p2, p3);
            }
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
            colliderMaxSizes.Dispose();
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            colliderBounds.Dispose();
            raycasterChunks.Dispose();
            raycasterOffsets.Dispose();
            raycastResultCounts.Dispose();
            colliderJoinedBounds.Dispose();
            Profiler.EndSample();
            
            SpaceDebug.LogState("ColliderCount", colliderCount);
        }

        private static float2 FindBoundsMaxSize(NativeArray<float2> sizes)
        {
            if (sizes.Length == 0)
            {
                return default;
            }

            var size = sizes[0];
            var xMax = size.x;
            var yMax = size.y;
            var colliderCount = sizes.Length;
            
            for (var i = 1; i < colliderCount; i++)
            {
                size = sizes[i];

                if (size.x > xMax)
                {
                    xMax = size.x;
                }

                if (size.y > yMax)
                {
                    yMax = size.y;
                }
            }

            return new float2(xMax, yMax);
        }

        private static ColliderBounds JoinBounds(NativeArray<ColliderBounds> colliders)
        {
            if (colliders.Length == 0)
            {
                return default;
            }

            var entityBounds = colliders[0];
            var xMin = entityBounds.xMin;
            var xMax = entityBounds.xMax;
            var yMin = entityBounds.yMin;
            var yMax = entityBounds.yMax;
            var colliderCount = colliders.Length;
            for (var i = 1; i < colliderCount; i++)
            {
                entityBounds = colliders[i];
                
                if (entityBounds.xMin < xMin)
                {
                    xMin = entityBounds.xMin;
                }

                if (entityBounds.yMin < yMin)
                {
                    yMin = entityBounds.yMin;
                }

                if (entityBounds.xMax > xMax)
                {
                    xMax = entityBounds.xMax;
                }

                if (entityBounds.yMax > yMax)
                {
                    yMax = entityBounds.yMax;
                }
            }
            
            return new ColliderBounds
            {
                xMin = xMin,
                xMax = xMax,
                yMin = yMin,
                yMax = yMax
            };
        }

        protected override void OnDestroy()
        {
            _entityBuffer.Dispose();
            _entityCount.Dispose();
        }
    }
}