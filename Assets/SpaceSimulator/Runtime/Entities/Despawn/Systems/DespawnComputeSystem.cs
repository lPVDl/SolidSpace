using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnComputeSystem : SystemBase
    {
        private const int IterationCycle = 2;

        public NativeArray<Entity> ResultBuffer => _entityBufferB;
        public int ResultCount => _collectorCountBuffer[0];
        
        private NativeArray<int> _countsBuffer;
        private NativeArray<int> _collectorCountBuffer;
        private NativeArray<Entity> _entityBufferA;
        private NativeArray<Entity> _entityBufferB;
        private EntityQuery _query;
        private int _lastOffset;
        
        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(typeof(DespawnComponent));
            _lastOffset = -1;
            _countsBuffer = CreatePersistentArray<int>(128);
            _entityBufferA = CreatePersistentArray<Entity>(4096);
            _entityBufferB = CreatePersistentArray<Entity>(4096);
            _collectorCountBuffer = CreatePersistentArray<int>(1);
            _collectorCountBuffer[0] = 0;
        }

        protected override void OnUpdate()
        {
            var chunkCount = _query.CalculateChunkCount();
            if (chunkCount == 0)
            {
                return;
            }

            Profiler.BeginSample("Create Compute Buffer");
            _lastOffset = (_lastOffset + 1) % IterationCycle;
            var rawChunks = _query.CreateArchetypeChunkArray(Allocator.Temp);
            var computeChunkCount = Mathf.CeilToInt((chunkCount - _lastOffset) / (float) IterationCycle);
            var computeChunks = CreateTempJobArray<ArchetypeChunk>(computeChunkCount);
            var computeOffsets = CreateTempJobArray<int>(computeChunkCount);
            var entityCount = 0;
            var chunkIndex = 0;

            for (var offset = _lastOffset; offset < chunkCount; offset += IterationCycle)
            {
                var rawChunk = rawChunks[offset];
                computeOffsets[chunkIndex] = entityCount;
                computeChunks[chunkIndex] = rawChunk;
                entityCount += rawChunk.Count;
                chunkIndex++;
            }
            
            rawChunks.Dispose();
            Profiler.EndSample();

            Profiler.BeginSample("Update Counts Buffer");
            if (_countsBuffer.Length < computeChunkCount)
            {
                _countsBuffer.Dispose();
                _countsBuffer = CreatePersistentArray<int>(computeChunkCount * 2);
            }
            Profiler.EndSample();

            Profiler.BeginSample("Update Entities Buffer");
            if (_entityBufferA.Length < entityCount)
            {
                _entityBufferA.Dispose();
                _entityBufferA = CreatePersistentArray<Entity>(entityCount * 2);
                _entityBufferB.Dispose();
                _entityBufferB = CreatePersistentArray<Entity>(entityCount * 2);
            }
            Profiler.EndSample();

            var computeJob = new DespawnComputeJob
            {
                chunks = computeChunks,
                despawnHandle = GetComponentTypeHandle<DespawnComponent>(true),
                entityHandle = GetEntityTypeHandle(),
                resultCounts = _countsBuffer, 
                offsets = computeOffsets,
                resultEntities = _entityBufferA,
                time = (float)Time.ElapsedTime
            };
            var computeJobHandle = computeJob.Schedule(computeChunkCount, 32, Dependency);

            var collectJob = new DespawnCollectJob
            {
                inputCountsAmount = computeChunkCount,
                inputCounts = _countsBuffer,
                inputEntities = _entityBufferA,
                offsets = computeOffsets,
                outputEntities = _entityBufferB,
                outputCount = _collectorCountBuffer
            };
            var collectJobHandle = collectJob.Schedule(computeJobHandle);
            
            collectJobHandle.Complete();
        }

        private static NativeArray<T> CreatePersistentArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        private static NativeArray<T> CreateTempJobArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        protected override void OnDestroy()
        {
            _countsBuffer.Dispose();
            _entityBufferA.Dispose();
            _entityBufferB.Dispose();
            _collectorCountBuffer.Dispose();
        }
    }
}