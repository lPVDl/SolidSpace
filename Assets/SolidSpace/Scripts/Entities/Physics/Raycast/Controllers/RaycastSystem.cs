using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Utilities;
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
            var chunkOffsets = EntityQueryForJobUtil.ComputeOffsets(archetypeChunks);
            Profiler.EndSample("Chunk offsets");

            Profiler.BeginSample("Raycast");
            behaviour.Initialize(chunkOffsets.entityCount);
            var raycastJob = new RaycastJob<T>
            {
                behaviour = behaviour,
                hitStackSize = HitStackSize,
                inArchetypeChunks = archetypeChunks,
                inColliders = colliders,
                inWriteOffsets = chunkOffsets.chunkOffsets,
                hitStack = NativeMemory.CreateTempJobArray<ushort>(chunkOffsets.chunkCount * HitStackSize),
                outCounts = NativeMemory.CreateTempJobArray<int>(chunkOffsets.chunkCount)
            };
            raycastJob.Schedule(chunkOffsets.chunkCount, 1).Complete();
            Profiler.EndSample("Raycast");
            
            Profiler.BeginSample("Collect results");
            behaviour.CollectResult(chunkOffsets.chunkCount, chunkOffsets.chunkOffsets, raycastJob.outCounts);
            Profiler.EndSample("Collect results");

            Profiler.BeginSample("Dispose arrays");
            raycastJob.hitStack.Dispose();
            raycastJob.outCounts.Dispose();
            chunkOffsets.chunkOffsets.Dispose();
            Profiler.EndSample("Dispose arrays");
        }
    }
}