using System;
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
        private const int MaxChunksPerCollider = 4;
        private const float MinCellSize = 1;
        
        private EntityQuery _query;
        private SystemBaseUtil _systemUtil;
        private GridUtil _gridUtil;
        private NativeArray<ColliderBounds> _colliderBounds;
        private NativeArray<ushort> _worldColliders;
        private NativeArray<ColliderListPointer> _worldChunks;
        private NativeArray<ChunkColliderBounds> _colliderBoundsStream; 

        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            });
            _colliderBounds = _systemUtil.CreatePersistentArray<ColliderBounds>(ColliderBufferChunkSize);
            _worldColliders = _systemUtil.CreatePersistentArray<ushort>(ColliderBufferChunkSize * MaxChunksPerCollider);
            _worldChunks = _systemUtil.CreatePersistentArray<ColliderListPointer>(ChunkBufferChunkSize);
            _colliderBoundsStream = _systemUtil.CreatePersistentArray<ChunkColliderBounds>(ColliderBufferChunkSize * MaxChunksPerCollider);
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

            var worldGrid = _gridUtil.ComputeGrid(_colliderBounds, colliderCount);
            DebugDrawWorld(worldGrid);

            Profiler.BeginSample("Bake Chunks");
            var worldChunkTotal = worldGrid.cellCount.x * worldGrid.cellCount.y;
            _systemUtil.MaintainPersistentArrayLength(ref _worldChunks, worldChunkTotal, ChunkBufferChunkSize);

            Profiler.BeginSample("Reset Chunks");
            var resetChunksJob = new FillNativeArrayJob<ColliderListPointer>
            {
                inItemPerJob = 128,
                inValue = default,
                inTotalItem = worldChunkTotal,
                outNativeArray = _worldChunks
            };
            var jobCount = (int) math.ceil(worldChunkTotal / 128f);
            var resetChunksJobHandle = resetChunksJob.Schedule(jobCount, 1, Dependency);
            resetChunksJobHandle.Complete();
            Profiler.EndSample();

            _systemUtil.MaintainPersistentArrayLength(ref _colliderBoundsStream, colliderCount * MaxChunksPerCollider,
                ColliderBufferChunkSize * MaxChunksPerCollider);
            
            Profiler.BeginSample("Bounds stream");
            var boundsStreamIndex = 0;
            for (var i = 0; i < colliderCount; i++)
            {
                var bounds = _colliderBounds[i];
                var x0 = (int) math.min(worldGrid.cellCount.x - 1, math.floor((bounds.xMin - worldGrid.worldMin.x) / worldGrid.cellSize.x));
                var y0 = (int) math.min(worldGrid.cellCount.y - 1, math.floor((bounds.yMin - worldGrid.worldMin.y) / worldGrid.cellSize.y));
                var x1 = (int) math.min(worldGrid.cellCount.x - 1, math.floor((bounds.xMax - worldGrid.worldMin.x) / worldGrid.cellSize.x));
                var y1 = (int) math.min(worldGrid.cellCount.y - 1, math.floor((bounds.yMax - worldGrid.worldMin.y) / worldGrid.cellSize.y));

                AddCollider(y0 * worldGrid.cellCount.x + x0, i);

                if (x0 != x1)
                {
                    AddCollider(y0 * worldGrid.cellCount.x + x1, i);

                    if (y0 != y1)
                    {
                        AddCollider(y1 * worldGrid.cellCount.x + x1, i);
                    }
                }
                
                if (y0 != y1)
                {
                    AddCollider(y1 * worldGrid.cellCount.x + x0, i);
                }
            }
            
            SpaceDebug.LogState("BoundsStreamIndex", boundsStreamIndex);

            void AddCollider(int chunkIndex, int colliderIndex)
            {
                var chunkData = _worldChunks[chunkIndex];
                chunkData.count++;
                _worldChunks[chunkIndex] = chunkData;
                
                _colliderBoundsStream[boundsStreamIndex++] = new ChunkColliderBounds
                {
                    chunkIndex = (ushort) chunkIndex,
                    colliderIndex = (ushort) colliderIndex
                };
            }
            
            Profiler.EndSample();

            Profiler.EndSample();
            
            Profiler.BeginSample("Dispose arrays");
            colliderChunks.Dispose();
            colliderOffsets.Dispose();
            Profiler.EndSample();

            ColliderWorld = new ColliderWorld
            {
                colliders = new NativeSlice<ColliderBounds>(_colliderBounds, colliderCount)
            };

            SpaceDebug.LogState("ColliderCount", colliderCount);
        }

        private void DebugDrawWorld(ColliderWorldGrid worldGrid)
        {
            var cellCountX = worldGrid.cellCount.x;
            var cellCountY = worldGrid.cellCount.y;
            var cellSizeX = worldGrid.cellSize.x;
            var cellSizeY = worldGrid.cellSize.y;
            var worldMin = worldGrid.worldMin;
            var worldMax = worldGrid.worldMax;
            
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
        }

        protected override void OnDestroy()
        {
            _colliderBounds.Dispose();
            _worldColliders.Dispose();
            _worldChunks.Dispose();
            _colliderBoundsStream.Dispose();
        }
    }
}