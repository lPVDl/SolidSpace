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
        public ColliderWorld World { get; private set; }
        
        private const float MinWorldSize = 1;
        private const float MinCellSize = 1;
        private const int MaxWorldCellCount = 128;
        private const int ColliderBufferChunkSize = 512;
        
        private EntityQuery _query;
        private SystemBaseUtil _systemUtil;
        private BoundsUtil _boundsUtil;
        private NativeArray<ColliderBounds> _colliderBounds;
        
        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = _systemUtil.CreatePersistentArray<ColliderBounds>(ColliderBufferChunkSize);
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
            var computeBoundsJobHandle = computeBoundsJob.Schedule(colliderChunkCount, 16, Dependency);
            computeBoundsJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Compute World Bounds");
            var colliderJobCount = (int) (colliderCount / 128f + 0.5);
            var colliderJoinedBounds = _systemUtil.CreateTempJobArray<ColliderBounds>(colliderJobCount);
            var worldBoundsJob = new JoinBoundsJob
            {
                inBounds = _colliderBounds,
                inBoundsPerJob = 128,
                outBounds = colliderJoinedBounds
            };
            var worldBoundsJobHandle = worldBoundsJob.Schedule(colliderJobCount, 1, Dependency);

            Profiler.BeginSample("Compute World Bounds : Jobs");
            worldBoundsJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Compute World Bounds : Main Thread");
            var worldBounds = _boundsUtil.JoinBounds(colliderJoinedBounds);
            Profiler.EndSample();

            var colliderMaxSizes = _systemUtil.CreateTempJobArray<float2>(colliderJobCount);
            var colliderSizesJob = new FindMaxColliderSizeJob
            {
                inBounds = _colliderBounds,
                inBoundsPerJob = 128,
                outSizes = colliderMaxSizes
            };
            var colliderSizesJobHandle = colliderSizesJob.Schedule(colliderJobCount, 1, Dependency);

            Profiler.BeginSample("Find Max Collider Size : Jobs");
            colliderSizesJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Find Max Collider Size : Main Thread");
            var maxColliderSizes = _boundsUtil.FindBoundsMaxSize(colliderMaxSizes);
            var worldCellSize = math.max(math.max(maxColliderSizes.x, maxColliderSizes.y), MinCellSize);
            Profiler.EndSample();

            var worldMin = new float2(worldBounds.xMin, worldBounds.yMin);
            var worldMax = new float2(worldBounds.xMax, worldBounds.yMax);
            var worldDelta = worldMax - worldMin;
            var worldSize = math.max(math.max(worldDelta.x, worldDelta.y), MinWorldSize);
            var cellCount = (int) (worldSize / worldCellSize + 0.5);
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

            Profiler.BeginSample("Dispose arrays");
            colliderMaxSizes.Dispose();
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            colliderJoinedBounds.Dispose();
            Profiler.EndSample();

            World = new ColliderWorld
            {
                colliders = new NativeSlice<ColliderBounds>(_colliderBounds, colliderCount)
            };

            SpaceDebug.LogState("ColliderCount", colliderCount);
        }

        protected override void OnDestroy()
        {
            _colliderBounds.Dispose();
        }
    }
}