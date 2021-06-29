using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Raycast;
using SolidSpace.Entities.World;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Colliders
{
    public class ColliderBakeSystem<T> : IColliderBakeSystem<T> 
        where T : struct, IColliderBakeBehaviour
    {
        public ProfilingHandle Profiler { get; set; }
        public EntityQuery Query { get; set; }
        public IEntityManager EntityManager { get; set; }

        public BakedColliders Bake(ref T behaviour)
        {
            Profiler.BeginSample("Query archetype chunks");
            var archetypeChunks = Query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample("Query archetype chunks");

            Profiler.BeginSample("Compute offsets");
            var archetypeChunkCount = archetypeChunks.Length;
            var archetypeChunkOffsets = NativeMemory.CreateTempJobArray<int>(archetypeChunkCount);
            var colliderCount = 0;
            if (colliderCount > ushort.MaxValue)
            {
                throw new InvalidOperationException($"Collider count exceeded max value ({ushort.MaxValue})");
            }
            for (var i = 0; i < archetypeChunkCount; i++)
            {
                archetypeChunkOffsets[i] = colliderCount;
                colliderCount += archetypeChunks[i].Count;
            }
            Profiler.EndSample("Compute offsets");
            
            Profiler.BeginSample("Collect data");
            behaviour.Initialize(colliderCount);
            var dataCollectJob = new KovacDataCollectJob<T>
            {
                behaviour = behaviour,
                inArchetypeChunks = archetypeChunks,
                inWriteOffsets = archetypeChunkOffsets,
                positionHandle = EntityManager.GetComponentTypeHandle<PositionComponent>(true),
                rotationHandle = EntityManager.GetComponentTypeHandle<RotationComponent>(true),
                rectSizeHandle = EntityManager.GetComponentTypeHandle<RectSizeComponent>(true),
                outShapes = NativeMemory.CreateTempJobArray<ColliderShape>(colliderCount),
                outBounds = NativeMemory.CreateTempJobArray<FloatBounds>(colliderCount),
            };
            dataCollectJob.Schedule(archetypeChunkCount, 8).Complete();
            Profiler.EndSample("Collect data");
            
            Profiler.BeginSample("Construct grid");
            var worldGrid = ColliderGridUtil.Static_ComputeGrid(dataCollectJob.outBounds, colliderCount, Profiler);
            Profiler.EndSample("Construct grid");
            
            Profiler.BeginSample("Allocate cells");
            var worldCellTotal = worldGrid.size.x * worldGrid.size.y;
            var worldCells = NativeMemory.CreateTempJobArray<ColliderListPointer>(worldCellTotal);
            new FillNativeArrayJob<ColliderListPointer>
            {
                inItemPerJob = 128,
                inValue = default,
                inTotalItem = worldCellTotal,
                outNativeArray = worldCells
            }.Schedule((int) Math.Ceiling(worldCellTotal / 128f), 8).Complete();
            Profiler.EndSample("Allocate cells");
            
            Profiler.BeginSample("Bake colliders");
            var jobCount = (int) Math.Ceiling(colliderCount / 128f);
            var bakingJob = new ChunkCollidersJob
            {
                inColliderBounds = dataCollectJob.outBounds,
                inWorldGrid = worldGrid,
                inColliderPerJob = 128,
                inColliderTotalCount = colliderCount,
                outColliders = NativeMemory.CreateTempJobArray<ChunkedCollider>(colliderCount * 4),
                outColliderCounts = NativeMemory.CreateTempJobArray<int>(jobCount)
            };
            bakingJob.Schedule(jobCount, 8).Complete();
            Profiler.EndSample("Bake colliders");
            
            Profiler.BeginSample("Lists capacity");
            new ChunkListsCapacityJob
            {
                inColliderBatchCapacity = 128 * 4,
                inColliders = bakingJob.outColliders,
                inColliderCounts = bakingJob.outColliderCounts,
                inOutLists = worldCells
            }.Schedule().Complete();
            Profiler.EndSample("Lists capacity");
            
            Profiler.BeginSample("Lists offsets");
            new ChunkListsOffsetJob
            {
                inListCount = worldCellTotal,
                inOutLists = worldCells
            }.Schedule().Complete();
            Profiler.EndSample("Lists offsets");

            Profiler.BeginSample("Lists fill");
            var listsFillJob = new ChunkListsFillJob
            {
                inColliderBatchCapacity = 128 * 4,
                inColliders = bakingJob.outColliders,
                inColliderCounts = bakingJob.outColliderCounts,
                inOutLists = worldCells,
                outColliders = NativeMemory.CreateTempJobArray<ushort>(colliderCount * 4)
            };
            listsFillJob.Schedule().Complete();
            Profiler.EndSample("Lists fill");

            Profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            archetypeChunkOffsets.Dispose();
            bakingJob.outColliders.Dispose();
            bakingJob.outColliderCounts.Dispose();
            Profiler.EndSample("Dispose arrays");

            return new BakedColliders
            {
                shapes = dataCollectJob.outShapes,
                bounds = dataCollectJob.outBounds,
                grid = worldGrid,
                cells = worldCells,
                indices = listsFillJob.outColliders
            };
        }
    }
}