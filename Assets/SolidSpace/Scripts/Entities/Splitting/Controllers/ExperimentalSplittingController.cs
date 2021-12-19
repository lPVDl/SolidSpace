using System.Collections.Generic;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Splitting
{
    public class ExperimentalSplittingController : ISplittingCommandSystem, IInitializable, IUpdatable
    {
        private readonly IEntityDestructionBuffer _destructionBuffer;
        private readonly IEntityManager _entityManager;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IProfilingManager _profilingManager;
        private ProfilingHandle _profiler;

        private HashSet<Entity> _splittingQueue;

        public ExperimentalSplittingController(IEntityManager entityManager,
                                               IProfilingManager profilingManager,
                                               IHealthAtlasSystem healthSystem,
                                               IEntityDestructionBuffer destructionBuffer)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _healthSystem = healthSystem;
            _destructionBuffer = destructionBuffer;
        }

        public void OnInitialize()
        {
            _splittingQueue = new HashSet<Entity>();
            _profiler = _profilingManager.GetHandle(this);
        }

        public void OnFinalize()
        {
        }

        public void ScheduleSplittingCheck(Entity entity)
        {
            _splittingQueue.Add(entity);
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Collect entity data");
            var entityCount = _splittingQueue.Count;
            var seedMaskSize = 0;
            var entitiesData = NativeMemory.CreateTempJobArray<SplittingEntityData>(entityCount);
            var entityIndex = 0;

            foreach (var entity in _splittingQueue)
            {
                var entitySize = _entityManager.GetComponentData<RectSizeComponent>(entity).value;
                var entitySizeInt = new int2((int)entitySize.x, (int)entitySize.y);
                var healthIndex = _entityManager.GetComponentData<HealthComponent>(entity).index;
                var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks[healthIndex.ReadChunkId()], healthIndex);

                entitiesData[entityIndex] = new SplittingEntityData
                {
                    entity = entity,
                    seedMaskOffset = seedMaskSize,
                    healthAtlasOffset = healthOffset,
                    entitySize = entitySizeInt
                };

                seedMaskSize += entitySizeInt.x * entitySizeInt.y;

                entityIndex++;
            }

            _splittingQueue.Clear();
            _profiler.EndSample("Collect entity data");

            _profiler.BeginSample("Allocate arrays");
            var connections = NativeMemory.CreateTempJobArray<byte2>(256 * entityCount);
            var seedResults = NativeMemory.CreateTempJobArray<ShapeSeedJobResult>(entityCount);
            var bounds = NativeMemory.CreateTempJobArray<ByteBounds>(256 * entityCount);
            var seedMask = NativeMemory.CreateTempJobArray<byte>(seedMaskSize);
            var shapeCounts = NativeMemory.CreateTempJobArray<int>(entityCount);
            var shapeRootSeeds = NativeMemory.CreateTempJobArray<byte>(256 * entityCount);
            var jobHandles = NativeMemory.CreateTempJobArray<JobHandle>(entityCount);
            _profiler.EndSample("Allocate arrays");

            _profiler.BeginSample("Schedule jobs");
            for (var i = 0; i < entityCount; i++)
            {
                var entity = entitiesData[i];
                var frameLength = HealthFrameBitsUtil.GetRequiredByteCount(entity.entitySize.x, entity.entitySize.y);
                var maskSize = entity.entitySize.x * entity.entitySize.y;

                var seedJob = new ShapeSeedJob
                {
                    inFrameBits = new NativeSlice<byte>(_healthSystem.Data, entity.healthAtlasOffset, frameLength),
                    inFrameSize = entity.entitySize,
                    outConnections = new NativeSlice<byte2>(connections, i * 256, 256),
                    outResult = new NativeSlice<ShapeSeedJobResult>(seedResults, i, 1),
                    outSeedBounds = new NativeSlice<ByteBounds>(bounds, i * 256, 256),
                    outSeedMask = new NativeSlice<byte>(seedMask, entity.seedMaskOffset, maskSize)
                };

                var readJob = new ShapeReadJob
                {
                    inOutConnections = seedJob.outConnections,
                    inOutBounds = seedJob.outSeedBounds,
                    inSeedJobResult = seedJob.outResult,
                    outShapeCount = new NativeSlice<int>(shapeCounts, i, 1),
                    outShapeRootSeeds = new NativeSlice<byte>(shapeRootSeeds, i * 256, 256)
                };

                jobHandles[i] = readJob.Schedule(seedJob.Schedule());
            }

            _profiler.EndSample("Schedule jobs");

            _profiler.BeginSample("Complete jobs");
            JobHandle.CombineDependencies(jobHandles).Complete();
            _profiler.EndSample("Complete jobs");

            _profiler.BeginSample("Schedule replication");
            for (var i = 0; i < entityCount; i++)
            {
                var seedResult = seedResults[i];
                if (seedResult.code != EShapeSeedResult.Success)
                {
                    Debug.LogError($"Seeding job ended with result '{seedResult.code}'");
                }

                var entity = entitiesData[i];
                var shapeCount = shapeCounts[i];
                if (shapeCount == 0)
                {
                    _destructionBuffer.ScheduleDestroy(entity.entity);
                    continue;
                }

                if (shapeCount == 1)
                {
                    var childBounds = bounds[0];
                    if (entity.entitySize.x == childBounds.max.x - childBounds.min.x + 1 &&
                        entity.entitySize.y == childBounds.max.y - childBounds.min.y + 1)
                        continue;
                }

                // TODO : Replication!
                // Seed mask
                // Seed connections
                // Seed connection count
                // Parent entity
                // Parent entity size
                // 
            }

            _profiler.EndSample("Schedule replication");

            _profiler.BeginSample("Disposal");
            connections.Dispose();
            seedResults.Dispose();
            bounds.Dispose();
            seedMask.Dispose();
            shapeCounts.Dispose();
            shapeRootSeeds.Dispose();
            jobHandles.Dispose();
            _profiler.EndSample("Disposal");
        }
    }
}