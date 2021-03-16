using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.Randomization
{
    public class RandomValueSystem : SystemBase
    {
        private const int BufferChunkSize = 1024;
        
        private NativeArray<float> _randomBuffer;
        private int _randomIndex;
        private EntityQuery _query;

        protected override void OnStartRunning()
        {
            _randomBuffer = new NativeArray<float>(BufferChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < BufferChunkSize; i++)
            {
                _randomBuffer[i] = Random.value;
            }
            _query = EntityManager.CreateEntityQuery(typeof(RandomValueComponent));
        }

        protected override void OnUpdate()
        {
            var entityCount = _query.CalculateEntityCount();
            var requiredBufferCapacity = Mathf.CeilToInt(entityCount / (float) BufferChunkSize) * BufferChunkSize;
            if (_randomBuffer.Length < requiredBufferCapacity)
            {
                var newRandomBuffer = new NativeArray<float>(requiredBufferCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (var i = 0; i < _randomBuffer.Length; i++)
                {
                    newRandomBuffer[i] = _randomBuffer[i];
                }
                for (var i = _randomBuffer.Length; i < newRandomBuffer.Length; i++)
                {
                    newRandomBuffer[i] = Random.value;
                }
                _randomBuffer.Dispose();
                _randomBuffer = newRandomBuffer;
            }
            
            _randomIndex = (_randomIndex + 1) % _randomBuffer.Length;
            _randomBuffer[_randomIndex] = Random.value;
            
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new RandomValueJob
            {
                chunks = chunks,
                randomHandle = GetComponentTypeHandle<RandomValueComponent>(),
                randomIndex = _randomIndex,
                randomValues = _randomBuffer
            };
            var handle = job.Schedule(chunks.Length, 32, Dependency);
            
            handle.Complete();
        }

        protected override void OnDestroy()
        {
            _randomBuffer.Dispose();
        }
    }
}