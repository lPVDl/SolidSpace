using SpaceSimulator.Runtime.Entities.Extensions;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    partial class ColliderBakeSystem
    {
        private struct GridUtil
        {
            private BoundsUtil _boundsUtil;
            private SystemBaseUtil _systemUtil;

            public ColliderWorldGrid ComputeGrid(NativeArray<ColliderBounds> colliders, int colliderCount)
            {
                Profiler.BeginSample("ComputeGrid");
                
                var worldBounds = ComputeWorldBounds(colliders, colliderCount);
                var maxColliderSize = FindMaxColliderSize(colliders, colliderCount);
                
                var cellSize = math.max(1, math.max(maxColliderSize.x, maxColliderSize.y));
                var cellPower = (int) math.ceil(math.log2(cellSize));
                var worldMin = new int2((int) worldBounds.xMin >> cellPower, (int) worldBounds.yMin >> cellPower);
                var worldMax = new int2((int) worldBounds.xMax >> cellPower, (int) worldBounds.yMax >> cellPower);
                
                var cellTotal = (worldMax.x - worldMin.x + 1) * (worldMax.y - worldMin.y + 1);
                if (cellTotal > MaxCellCount)
                {
                    cellSize = (1 << cellPower) / math.sqrt(MaxCellCount / (float) cellTotal);
                    cellPower = (int) math.ceil(math.log2(cellSize));
                    worldMin = new int2((int) worldBounds.xMin >> cellPower, (int) worldBounds.yMin >> cellPower);
                    worldMax = new int2((int) worldBounds.xMax >> cellPower, (int) worldBounds.yMax >> cellPower);
                }

                Profiler.EndSample();

                return new ColliderWorldGrid
                {
                    anchor = worldMin,
                    size = worldMax - worldMin + new int2(1, 1),
                    power = cellPower
                };
            }

            private ColliderBounds ComputeWorldBounds(NativeArray<ColliderBounds> colliders, int colliderCount)
            {
                Profiler.BeginSample("ComputeWorldBounds");
                
                var colliderJobCount = (int)math.ceil(colliderCount / 128f);
                var colliderJoinedBounds = _systemUtil.CreateTempJobArray<ColliderBounds>(colliderJobCount);
                var worldBoundsJob = new JoinBoundsJob
                {
                    inBounds = colliders,
                    inBoundsPerJob = 128,
                    inTotalBounds = colliderCount,
                    outBounds = colliderJoinedBounds
                };
                var worldBoundsJobHandle = worldBoundsJob.Schedule(colliderJobCount, 1);

                Profiler.BeginSample("Jobs");
                worldBoundsJobHandle.Complete();
                Profiler.EndSample();

                Profiler.BeginSample("Main Thread");
                var worldBounds = _boundsUtil.JoinBounds(colliderJoinedBounds);
                Profiler.EndSample();

                colliderJoinedBounds.Dispose();
                
                Profiler.EndSample();

                return worldBounds;
            }

            private float2 FindMaxColliderSize(NativeArray<ColliderBounds> colliders, int colliderCount)
            {
                Profiler.BeginSample("FindMaxColliderSize");
                
                var colliderJobCount = (int)math.ceil(colliderCount / 128f);
                var colliderMaxSizes = _systemUtil.CreateTempJobArray<float2>(colliderJobCount);
                var colliderSizesJob = new FindMaxColliderSizeJob
                {
                    inBounds = colliders,
                    inBoundsPerJob = 128,
                    inTotalBounds = colliderCount,
                    outSizes = colliderMaxSizes
                };
                var colliderSizesJobHandle = colliderSizesJob.Schedule(colliderJobCount, 1);

                Profiler.BeginSample("Find Max Collider Size : Jobs");
                colliderSizesJobHandle.Complete();
                Profiler.EndSample();

                Profiler.BeginSample("Find Max Collider Size : Main Thread");
                var maxColliderSize = _boundsUtil.FindBoundsMaxSize(colliderMaxSizes);
                Profiler.EndSample();

                colliderMaxSizes.Dispose();
                
                Profiler.EndSample();

                return maxColliderSize;
            }
        }
    }
}