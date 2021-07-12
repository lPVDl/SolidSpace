using System.Runtime.CompilerServices;
using SolidSpace.JobUtilities;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Utilities
{
    public static class EntityQueryUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArchetypeChunksWithOffsets QueryWithOffsetsForJob(EntityQuery entityQuery)
        {
            var chunks = entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var chunkCount = chunks.Length;
            var offsets = NativeMemory.CreateTempJobArray<int>(chunkCount);
            var entityCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = entityCount;
                entityCount += chunks[i].Count;
            }

            return new ArchetypeChunksWithOffsets
            {
                archetypeChunks = chunks,
                chunkOffsets = offsets,
                chunkCount = chunkCount,
                entityCount = entityCount
            };
        }
    }
}