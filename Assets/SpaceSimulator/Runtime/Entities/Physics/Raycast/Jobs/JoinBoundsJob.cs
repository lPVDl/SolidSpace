using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Physics.Raycast
{
    [BurstCompile]
    public struct JoinBoundsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ColliderBounds> inBounds;
        [ReadOnly] public int inBoundsPerJob;
        
        [WriteOnly] public NativeArray<ColliderBounds> outBounds;

        public void Execute(int jobIndex)
        {
            var startIndex = inBoundsPerJob * jobIndex;
            var endIndex = math.min(startIndex + inBoundsPerJob, inBounds.Length);
            var bounds = inBounds[startIndex];
            var xMin = bounds.xMin;
            var xMax = bounds.xMax;
            var yMin = bounds.yMin;
            var yMax = bounds.yMax;

            for (var i = startIndex + 1; i < endIndex; i++)
            {
                bounds = inBounds[startIndex];

                if (bounds.xMin < xMin)
                {
                    xMin = bounds.xMin;
                }

                if (bounds.yMin < yMin)
                {
                    yMin = bounds.yMin;
                }

                if (bounds.xMax > xMax)
                {
                    xMax = bounds.xMax;
                }

                if (bounds.yMax > yMax)
                {
                    yMax = bounds.yMax;
                }
            }

            outBounds[jobIndex] = new ColliderBounds
            {
                xMin = xMin,
                xMax = xMax,
                yMin = yMin,
                yMax = yMax
            };
        }
    }
}