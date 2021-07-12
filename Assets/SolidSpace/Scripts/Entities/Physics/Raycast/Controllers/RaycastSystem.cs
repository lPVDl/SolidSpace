using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Raycast
{
    public class RaycastSystem<T> : IRaycastSystem<T> where T : struct, IRaycastBehaviour
    {
        private const int HitStackSize = 8;
        
        public ProfilingHandle Profiler { get; set; }

        public void Raycast(BakedColliders colliders, NativeArray<ArchetypeChunk> archetypeChunks, ref T behaviour)
        {
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
            var raycastJob = new RaycastJob<T>
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
            behaviour.CollectResult(archetypeChunkCount, archetypeChunkOffsets, raycastJob.outCounts);
            Profiler.EndSample("Collect results");

            Profiler.BeginSample("Dispose arrays");
            raycastJob.hitStack.Dispose();
            raycastJob.outCounts.Dispose();
            archetypeChunkOffsets.Dispose();
            Profiler.EndSample("Dispose arrays");
        }
    }
}