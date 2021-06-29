using System;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal static class ColliderGridUtil
    {
        private const int MaxCellCount = ushort.MaxValue;
        
        public static ColliderGrid Static_ComputeGrid(NativeArray<FloatBounds> colliders, int colliderCount, ProfilingHandle profiler)
        {
            profiler.BeginSample("World Bounds");
            var worldBounds = Static_ComputeWorldBounds(colliders, colliderCount, profiler);
            profiler.EndSample("World Bounds");
                
            profiler.BeginSample("Max Collider Size");
            var maxColliderSize = Static_FindMaxColliderSize(colliders, colliderCount, profiler);
            profiler.EndSample("Max Collider Size");
                
            var cellSize = Math.Max(1, Math.Max(maxColliderSize.x, maxColliderSize.y));
            var cellPower = (int) Math.Ceiling(Math.Log(cellSize, 2));
            var worldMin = new int2((int) worldBounds.xMin >> cellPower, (int) worldBounds.yMin >> cellPower);
            var worldMax = new int2((int) worldBounds.xMax >> cellPower, (int) worldBounds.yMax >> cellPower);
                
            var cellTotal = (worldMax.x - worldMin.x + 1) * (worldMax.y - worldMin.y + 1);
            if (cellTotal > MaxCellCount)
            {
                cellSize = (1 << cellPower) / (float) Math.Sqrt(MaxCellCount / (float) cellTotal);
                cellPower = (int) Math.Ceiling(Math.Log(cellSize, 2));
                worldMin = new int2((int) worldBounds.xMin >> cellPower, (int) worldBounds.yMin >> cellPower);
                worldMax = new int2((int) worldBounds.xMax >> cellPower, (int) worldBounds.yMax >> cellPower);
            }

            return new ColliderGrid
            {
                anchor = worldMin,
                size = worldMax - worldMin + new int2(1, 1),
                power = cellPower
            };
        }

        private static FloatBounds Static_ComputeWorldBounds(NativeArray<FloatBounds> colliders, int colliderCount, ProfilingHandle profiler)
        {
            var colliderJobCount = (int) Math.Ceiling(colliderCount / 128f);
            var colliderJoinedBounds = NativeMemory.CreateTempJobArray<FloatBounds>(colliderJobCount);
            var worldBoundsJob = new JoinBoundsJob
            {
                inBounds = colliders,
                inBoundsPerJob = 128,
                inTotalBounds = colliderCount,
                outBounds = colliderJoinedBounds
            };
            var worldBoundsJobHandle = worldBoundsJob.Schedule(colliderJobCount, 1);

            profiler.BeginSample("Jobs");
            worldBoundsJobHandle.Complete();
            profiler.EndSample("Jobs");

            profiler.BeginSample("Main Thread");
            var worldBounds = ColliderBoundsUtil.Static_JoinBounds(colliderJoinedBounds);
            profiler.EndSample("Main Thread");

            colliderJoinedBounds.Dispose();

            return worldBounds;
        }

        private static float2 Static_FindMaxColliderSize(NativeArray<FloatBounds> colliders, int colliderCount, ProfilingHandle profiler)
        {
            var colliderJobCount = (int) Math.Ceiling(colliderCount / 128f);
            var colliderMaxSizes = NativeMemory.CreateTempJobArray<float2>(colliderJobCount);
            var colliderSizesJob = new FindMaxColliderSizeJob
            {
                inBounds = colliders,
                inBoundsPerJob = 128,
                inTotalBounds = colliderCount,
                outSizes = colliderMaxSizes
            };
            var colliderSizesJobHandle = colliderSizesJob.Schedule(colliderJobCount, 1);

            profiler.BeginSample("Jobs");
            colliderSizesJobHandle.Complete();
            profiler.EndSample("Jobs");

            profiler.BeginSample("Main Thread");
            var maxColliderSize = ColliderBoundsUtil.Static_FindBoundsMaxSize(colliderMaxSizes);
            profiler.EndSample("Main Thread");

            colliderMaxSizes.Dispose();
                
            return maxColliderSize;
        }
    }
}