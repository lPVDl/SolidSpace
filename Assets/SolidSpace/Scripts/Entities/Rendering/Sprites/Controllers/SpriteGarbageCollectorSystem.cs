using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public class SpriteGarbageCollectorSystem : IInitializable, IUpdatable
    {
        private readonly ISpriteColorSystem _spriteAtlas;
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        
        private EntityQuery _query;
        private ProfilingHandle _profiler;

        public SpriteGarbageCollectorSystem(ISpriteColorSystem spriteAtlas, IEntityManager entityManager,
            IProfilingManager profilingManager)
        {
            _spriteAtlas = spriteAtlas;
            _entityManager = entityManager;
            _profilingManager = profilingManager;
        }

        public void OnInitialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(SpriteRenderComponent)
            });
            _profiler = _profilingManager.GetHandle(this);
        }
        
        public void OnUpdate()
        {
            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
           
            _profiler.BeginSample("Create byte mask");
            var occupiedChunkCount = _spriteAtlas.ChunksOccupation.Length;
            var byteMaskSize = occupiedChunkCount * 16;
            var occupationByteMask = NativeMemory.CreateTempJobArray<byte>(byteMaskSize);
            var jobCount = (int) Math.Ceiling(byteMaskSize / 128f);
            new FillNativeArrayJob<byte>
            {
                inValue = 0,
                inTotalItem = byteMaskSize,
                inItemPerJob = 128,
                outNativeArray = occupationByteMask
            }.Schedule(jobCount, 4).Complete();
            _profiler.EndSample("Create byte mask");
            
            _profiler.BeginSample("Fill byte mask");
            new FillByteMaskWithSpriteIndexJob
            {
                inChunks = archetypeChunks,
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true),
                outMask = occupationByteMask
            }.Schedule(archetypeChunks.Length, 4).Complete();
            _profiler.EndSample("Fill byte mask");
            
            _profiler.BeginSample("Compare mask");
            var indicesToRelease = NativeMemory.CreateTempJobArray<AtlasIndex>(occupiedChunkCount * 16);
            var indicesCounts = NativeMemory.CreateTempJobArray<int>(occupiedChunkCount);
            new CompareAtlasMaskJob
            {
                inByteMask = occupationByteMask,
                inAtlasChunksOccupation = _spriteAtlas.ChunksOccupation,
                outRedundantIndices = indicesToRelease,
                outCounts = indicesCounts
            }.Schedule(occupiedChunkCount, 32).Complete();
            _profiler.EndSample("Compare mask");
            
            _profiler.BeginSample("Collect results");
            var indicesTotalCount = NativeMemory.CreateTempJobReference<int>(); 
            new DataCollectJob<AtlasIndex>
            {
                inCounts = indicesCounts,
                inOffset = 16,
                inOutData = indicesToRelease,
                outCount = indicesTotalCount
            }.Schedule().Complete();
            _profiler.EndSample("Collect results");
            
            _profiler.BeginSample("Release indices");
            var totalCount = indicesTotalCount.Value;
            for (var i = 0; i < totalCount; i++)
            {
                _spriteAtlas.Release(indicesToRelease[i]);
            }
            _profiler.EndSample("Release indices");
            
            _profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            occupationByteMask.Dispose();
            indicesToRelease.Dispose();
            indicesCounts.Dispose();
            indicesTotalCount.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void OnFinalize()
        {
            
        }
    }
}