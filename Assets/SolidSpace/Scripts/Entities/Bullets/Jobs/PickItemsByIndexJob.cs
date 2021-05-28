using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Bullets
{
    [BurstCompile]
    internal struct PickItemsByIndexJob<T> : IJob where T : struct
    {
        [ReadOnly] public NativeSlice<T> inItems;
        [ReadOnly] public NativeArray<int> inItemIndices;
        [ReadOnly] public int inItemIndexCount;

        [WriteOnly] public NativeArray<T> outItems;
        
        public void Execute()
        {
            for (var i = 0; i < inItemIndexCount; i++)
            {
                outItems[i] = inItems[inItemIndices[i]];
            }
        }
    }
}