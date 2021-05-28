using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Bullets
{
    [BurstCompile]
    internal struct RaycastWorldFilterJob : IJobParallelFor
    {
        [ReadOnly] public NativeHashSet<byte> inColliderArchetypesFilter;
        [ReadOnly] public NativeHashSet<byte> inRaycasterArchetypeFilter;
        [ReadOnly] public NativeSlice<byte> inColliderArchetypeIndices;
        [ReadOnly] public NativeSlice<byte> inRaycasterArchetypeIndices;
        [ReadOnly] public NativeSlice<ushort> inColliderIndices;
        [ReadOnly] public int inItemPerJob;
        [ReadOnly] public int inTotalItem;

        [WriteOnly] public NativeArray<int> outCounts;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> outIndices;
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inItemPerJob;
            var endIndex = Math.Min(startIndex + inItemPerJob, inTotalItem);
            var resultCount = 0;
            
            for (var i = startIndex; i < endIndex; i++)
            {
                var rayType = inRaycasterArchetypeIndices[i];
                if (!inRaycasterArchetypeFilter.Contains(rayType))
                {
                    continue;
                }

                var colliderType = inColliderArchetypeIndices[inColliderIndices[i]];
                if (!inColliderArchetypesFilter.Contains(colliderType))
                {
                    continue;
                }

                outIndices[startIndex + resultCount++] = i;
            }

            outCounts[jobIndex] = resultCount;
        }
    }
}