using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnComputeSystem : SystemBase
    {
        private const int IterationCycle = 8;

        public NativeArray<Entity> ResultBuffer => _entityBufferB;
        public int ResultCount => _collectorCountBuffer[0];
        
        private NativeArray<int> _countsBuffer;
        private NativeArray<int> _collectorCountBuffer;
        private NativeArray<Entity> _entityBufferA;
        private NativeArray<Entity> _entityBufferB;
        private EntityQuery _query;
        private int _offset;
        
        protected override void OnCreate()
        {
            _query = EntityManager.CreateEntityQuery(typeof(DespawnComponent));
            _offset = -1;
            _countsBuffer = CreatePersistentUninitializedNativeArray<int>(128);
            _entityBufferA = CreatePersistentUninitializedNativeArray<Entity>(4096);
            _entityBufferB = CreatePersistentUninitializedNativeArray<Entity>(4096);
            _collectorCountBuffer = CreatePersistentUninitializedNativeArray<int>(1);
            _collectorCountBuffer[0] = 0;
        }

        protected override void OnUpdate()
        {
            var chunkCount = _query.CalculateChunkCount();
            if (chunkCount == 0)
            {
                return;
            }

            var rawChunks = _query.CreateArchetypeChunkArray(Allocator.Temp);
            var computeChunkCountMax = Mathf.CeilToInt(chunkCount / (float) IterationCycle);
            var chunkCapacity = rawChunks[0].Capacity;
            var entityCountMax = computeChunkCountMax * chunkCapacity;

            if (_countsBuffer.Length < computeChunkCountMax)
            {
                _countsBuffer.Dispose();
                _countsBuffer = CreatePersistentUninitializedNativeArray<int>(computeChunkCountMax * 2);
            }

            if (_entityBufferA.Length < entityCountMax)
            {
                _entityBufferA.Dispose();
                _entityBufferA = CreatePersistentUninitializedNativeArray<Entity>(entityCountMax * 2);
                _entityBufferB.Dispose();
                _entityBufferB = CreatePersistentUninitializedNativeArray<Entity>(entityCountMax * 2);
            }

            _offset = (_offset + 1) % IterationCycle;
            
            var computeChunkCount = Mathf.CeilToInt((chunkCount - _offset) / (float) IterationCycle);
            var computeChunks = new NativeArray<ArchetypeChunk>(computeChunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var computeChunkIndex = 0;
            for (var chunkIndex = _offset; chunkIndex < chunkCount; chunkIndex += IterationCycle)
            {
                computeChunks[computeChunkIndex] = rawChunks[chunkIndex];
                computeChunkIndex++;
            }
            rawChunks.Dispose();
            
            var computeJob = new DespawnComputeJob
            {
                chunks = computeChunks,
                despawnHandle = GetComponentTypeHandle<DespawnComponent>(true),
                entityHandle = GetEntityTypeHandle(),
                resultCounts = _countsBuffer, 
                resultEntities = _entityBufferA,
                time = (float)Time.ElapsedTime
            };
            var computeJobHandle = computeJob.Schedule(computeChunkCount, 32, Dependency);

            var collectJob = new DespawnCollectJob
            {
                inputCountsAmount = computeChunkCount,
                inputCounts = _countsBuffer,
                inputEntities = _entityBufferA,
                inputEntitiesChunkSize = chunkCapacity,
                outputEntities = _entityBufferB,
                outputCount = _collectorCountBuffer
            };
            var collectJobHandle = collectJob.Schedule(computeJobHandle);
            
            collectJobHandle.Complete();
        }

        private static NativeArray<T> CreatePersistentUninitializedNativeArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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