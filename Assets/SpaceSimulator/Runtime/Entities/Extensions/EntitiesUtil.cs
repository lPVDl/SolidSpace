using Unity.Collections;
using Unity.Entities;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Extensions
{
    public static class EntitiesUtil
    {
        public static NativeArray<int> CreateEntityOffsetsForJob(NativeArray<ArchetypeChunk> chunks, out int entityCount)
        {
            Profiler.BeginSample("ComputeEntityOffsets");
            
            var chunkCount = chunks.Length;
            var offsets = new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            entityCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = entityCount;
                entityCount += chunks[i].Count;
            }
            
            Profiler.EndSample();

            return offsets;
        }
    }
}