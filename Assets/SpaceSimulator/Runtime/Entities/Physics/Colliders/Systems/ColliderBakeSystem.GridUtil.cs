using System;
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

                var cellSize = math.max(MinCellSize, math.max(maxColliderSize.x, maxColliderSize.y));
                var worldSizeX = math.max(1, worldBounds.xMax - worldBounds.xMin);
                var worldSizeY = math.max(1, worldBounds.yMax - worldBounds.yMin);
                var cellCountX = worldSizeX / cellSize;
                var cellCountY = worldSizeY / cellSize;
                var cellTotal = cellCountX * cellCountY;
                if (cellTotal > MaxCellCount)
                {
                    var downscaleFactor = math.sqrt(MaxCellCount / cellTotal);
                    cellCountX *= downscaleFactor;
                    cellCountY *= downscaleFactor;
                }

                var cellCountXInt = (int) math.max(1, math.floor(cellCountX));
                var cellCountYInt = (int) math.max(1, math.floor(cellCountY));
                ScaleGridAxis(ref cellCountXInt, ref cellCountYInt, MappingJobCount, MaxCellCount);

                var cellSizeX = math.max(cellSize, worldSizeX / cellCountXInt);
                var cellSizeY = math.max(cellSize, worldSizeY / cellCountYInt);
                var worldCenterX = (worldBounds.xMin + worldBounds.xMax) / 2;
                var worldCenterY = (worldBounds.yMin + worldBounds.yMax) / 2;
                var halfWorldSizeX = cellCountXInt * cellSizeX / 2;
                var halfWorldSizeY = cellCountYInt * cellSizeY / 2;
                var worldMin = new float2(worldCenterX - halfWorldSizeX, worldCenterY - halfWorldSizeY);
                var worldMax = new float2(worldCenterX + halfWorldSizeX, worldCenterY + halfWorldSizeY);

                Profiler.EndSample();

                return new ColliderWorldGrid
                {
                    cellSize = new float2(cellSizeX, cellSizeY),
                    cellCount = new int2(cellCountXInt, cellCountYInt),
                    worldMin = worldMin,
                    worldMax = worldMax
                };
            }

            private void ScaleGridAxis(ref int xCount, ref int yCount, int divider, int maxSquare)
            {
                const int safetyCounter = 256;

                var safe = 0;
                for (; (xCount * yCount) % divider != 0 && safe < safetyCounter; xCount++, yCount++, safe++)
                {
                    if (((xCount + 1) * yCount) % divider == 0)
                    {
                        xCount++;
                        break;
                    }

                    if ((xCount * (yCount + 1)) % divider == 0)
                    {
                        yCount++;
                        break;
                    }
                }

                if (safe >= safetyCounter)
                {
                    throw new InvalidOperationException("Failed to scale grid axis.");
                }

                var square = xCount * yCount;
                safe = 0;
                for (; ((square > maxSquare) || (square % divider != 0)) && (safe < safetyCounter); safe++)
                {
                    var tempX = math.max(1, xCount - 1);
                    square = tempX * yCount;
                    if ((square < maxSquare) && (square % divider == 0))
                    {
                        xCount = tempX;
                        break;
                    }

                    var tempY = math.max(1, yCount - 1);
                    square = xCount * tempY;
                    if ((square < maxSquare) && (square % divider == 0))
                    {
                        xCount = tempX;
                        break;
                    }

                    xCount = tempX;
                    yCount = tempY;
                }
                
                if (safe >= safetyCounter)
                {
                    throw new InvalidOperationException("Failed to scale grid axis.");
                }
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