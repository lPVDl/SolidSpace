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
        private const int MappingJobCount = 8;
        private const int MaxCellCount = 65536;
        private const int MinCellSize = 1;
        
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
            var colliderJobCount = (int)math.ceil(colliderCount / 128f);
            var colliderJoinedBounds = _systemUtil.CreateTempJobArray<ColliderBounds>(colliderJobCount);
            var worldBoundsJob = new JoinBoundsJob
            {
                inBounds = _colliderBounds,
                inBoundsPerJob = 128,
                inTotalBounds = colliderCount,
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
                inTotalBounds = colliderCount,
                outSizes = colliderMaxSizes
            };
            var colliderSizesJobHandle = colliderSizesJob.Schedule(colliderJobCount, 1, Dependency);

            Profiler.BeginSample("Find Max Collider Size : Jobs");
            colliderSizesJobHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Find Max Collider Size : Main Thread");
            var maxColliderSize = _boundsUtil.FindBoundsMaxSize(colliderMaxSizes);
            Profiler.EndSample();

            var cellSize = math.max(MinCellSize, math.max(maxColliderSize.x, maxColliderSize.y));
            var worldSizeX = math.max(1, worldBounds.xMax - worldBounds.xMin);
            var worldSizeY = math.max(1, worldBounds.yMax - worldBounds.yMin);
            var cellCountX = worldSizeX / cellSize;
            var cellCountY = worldSizeY / cellSize;
            var cellTotal = cellCountX * cellCountY;
            if (cellTotal > MaxCellCount)
            {
                var downscaleFactor = math.sqrt(MaxCellCount / cellTotal);
                cellCountX *= downscaleFactor;
                cellCountY *= downscaleFactor;
            }

            cellCountX = math.max(1, math.floor(cellCountX));
            cellCountY = math.max(1, math.floor(cellCountY));
            
            // cellCountX = math.max(1, math.floor(cellCountX / MappingJobCount)) * MappingJobCount;
            // cellCountY = math.max(1, math.floor(cellCountY / MappingJobCount)) * MappingJobCount;
            // cellTotal = cellCountX * cellCountY;
            // if (cellTotal > MaxCellCount)
            // {
            //     var overload = cellTotal - MaxCellCount;
            //     if (cellCountX > cellCountY)
            //     {
            //         cellCountX -= math.ceil(overload / cellCountY);
            //     }
            //     else
            //     {
            //         cellCountY -= math.ceil(overload / cellCountX);
            //     }
            // }

            var cellSizeX = math.max(cellSize, worldSizeX / cellCountX);
            var cellSizeY = math.max(cellSize, worldSizeY / cellCountY);
            var worldCenterX = (worldBounds.xMin + worldBounds.xMax) / 2;
            var worldCenterY = (worldBounds.yMin + worldBounds.yMax) / 2;
            var halfWorldSizeX = cellCountX * cellSizeX / 2;
            var halfWorldSizeY = cellCountY * cellSizeY / 2;
            var worldMin = new float2(worldCenterX - halfWorldSizeX, worldCenterY - halfWorldSizeY);
            var worldMax = new float2(worldCenterX + halfWorldSizeX, worldCenterY + halfWorldSizeY);
            
            SpaceDebug.LogState("ColliderCellCountX", cellCountX);
            SpaceDebug.LogState("ColliderCellCountY", cellCountY);
            SpaceDebug.LogState("ColliderCellSizeX", cellSizeX);
            SpaceDebug.LogState("ColliderCellSizeY", cellSizeY);
            
            for (var i = 1; i < cellCountX; i++)
            {
                var p0 = new Vector3(worldMin.x + cellSizeX * i, worldMax.y, 0);
                var p1 = new Vector3(worldMin.x + cellSizeX * i, worldMin.y, 0);
                Debug.DrawLine(p0, p1);
            }
            
            for (var i = 1; i < cellCountY; i++)
            {
                var p2 = new Vector3(worldMin.x, worldMin.y + i * cellSizeY, 0);
                var p3 = new Vector3(worldMax.x, worldMin.y + i * cellSizeY, 0);
                Debug.DrawLine(p2, p3);
            }
            
            Profiler.EndSample();

            Profiler.BeginSample("Dispose arrays");
            colliderMaxSizes.Dispose();
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            colliderJoinedBounds.Dispose();
            Profiler.EndSample();

            ColliderWorld = new ColliderWorld
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