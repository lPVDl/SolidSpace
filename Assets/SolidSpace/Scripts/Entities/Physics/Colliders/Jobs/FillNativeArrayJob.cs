using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    [BurstCompile]
    internal struct FillNativeArrayJob<T> : IJobParallelFor where T : struct
    {
        [ReadOnly] public T inValue;
        [ReadOnly] public int inItemPerJob;
        [ReadOnly] public int inTotalItem;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<T> outNativeArray;
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inItemPerJob;
            var endIndex = math.min(startIndex + inItemPerJob, inTotalItem);
            for (var i = startIndex; i < endIndex; i++)
            {
                outNativeArray[i] = inValue;
            }
        }
    }
}