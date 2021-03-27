using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    [BurstCompile]
    public struct FindMaxColliderSizeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ColliderBounds> inBounds;
        [ReadOnly] public int inBoundsPerJob;
        [ReadOnly] public int inTotalBounds;

        [WriteOnly] public NativeArray<float2> outSizes; 
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inBoundsPerJob;
            var endIndex = math.min(startIndex + inBoundsPerJob, inTotalBounds);
            
            var bounds = inBounds[startIndex];
            var xMax = bounds.xMax - bounds.xMin;
            var yMax = bounds.yMax - bounds.yMin;
            
            for (var i = startIndex + 1; i < endIndex; i++)
            {
                bounds = inBounds[i];
                var dX = bounds.xMax - bounds.xMin;
                var dY = bounds.yMax - bounds.yMin;
                
                if (dX > xMax)
                {
                    xMax = dX;
                }

                if (dY > yMax)
                {
                    yMax = dY;
                }
            }

            outSizes[jobIndex] = new float2(xMax, yMax);
        }
    }
}