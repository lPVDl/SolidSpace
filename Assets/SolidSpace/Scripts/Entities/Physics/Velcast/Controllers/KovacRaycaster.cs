using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Velcast
{
    public class KovacRaycaster<T> : IKovacRaycaster<T> where T : struct, IRaycastBehaviour
    {
        private const int HitStackSize = 8;

        public EntityQuery Query { get; set; }
        public ProfilingHandle Profiler { get; set; }

        public void Raycast(BakedCollidersData colliders, ref T behaviour)
        {
            Profiler.BeginSample("Query chunks");
            var archetypeChunks = Query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample("Query chunks");

            Profiler.BeginSample("Chunk offsets");
            var archetypeChunkCount = archetypeChunks.Length;
            var archetypeChunkOffsets = NativeMemory.CreateTempJobArray<int>(archetypeChunkCount);
            var entityCount = 0;
            for (var i = 0; i < archetypeChunkCount; i++)
            {
                archetypeChunkOffsets[i] = entityCount;
                entityCount += archetypeChunks[i].Count;
            }
            Profiler.EndSample("Chunk offsets");

            Profiler.BeginSample("Raycast");
            behaviour.Initialize(entityCount);
            var raycastJob = new KovacRaycastJob<T>
            {
                behaviour = behaviour,
                hitStackSize = HitStackSize,
                inArchetypeChunks = archetypeChunks,
                inColliders = colliders,
                inWriteOffsets = archetypeChunkOffsets,
                hitStack = NativeMemory.CreateTempJobArray<ushort>(archetypeChunkCount * HitStackSize),
                outCounts = NativeMemory.CreateTempJobArray<int>(archetypeChunkCount)
            };
            raycastJob.Schedule(archetypeChunkCount, 1).Complete();
            Profiler.EndSample("Raycast");
            
            Profiler.BeginSample("Collect results");
            behaviour.CollectResult(archetypeChunkOffsets, raycastJob.outCounts);
            Profiler.EndSample("Collect results");

            Profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            raycastJob.hitStack.Dispose();
            raycastJob.outCounts.Dispose();
            archetypeChunkOffsets.Dispose();
            Profiler.EndSample("Dispose arrays");
        }
    }
}