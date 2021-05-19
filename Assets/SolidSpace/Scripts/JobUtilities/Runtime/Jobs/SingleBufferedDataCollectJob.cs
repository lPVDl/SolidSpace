using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.JobUtilities
{
    [BurstCompile]
    public struct SingleBufferedDataCollectJob<T> : IJob where T : struct
    {
        public NativeArray<T> inOutData;
        
        [ReadOnly] public NativeArray<int> inOffsets;
        [ReadOnly] public NativeArray<int> inCounts;

        [WriteOnly] public NativeReference<int> outCount;
        
        public void Execute()
        {
            var resultCount = 0;
            var offsetCount = inOffsets.Length;
            for (var i = 0; i < offsetCount; i++)
            {
                var localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                var offset = inOffsets[i];
                for (var j = 0; j < localCount; j++)
                {
                    inOutData[resultCount] = inOutData[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }
}