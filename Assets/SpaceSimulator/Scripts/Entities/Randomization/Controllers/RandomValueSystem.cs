using SpaceSimulator.Entities.EntityWorld;
using SpaceSimulator.Entities.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SpaceSimulator.Entities.Randomization
{
    public class RandomValueSystem : IEntitySystem
    {
        private const int BufferChunkSize = 1024;

        public ESystemType SystemType => ESystemType.Compute;

        private readonly IEntityManager _entityManager;
        
        private NativeArray<float> _randomBuffer;
        private int _randomIndex;
        private EntityQuery _query;
        private EntitySystemUtil _util;

        public RandomValueSystem(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public void Initialize()
        {
            _randomBuffer = _util.CreatePersistentArray<float>(BufferChunkSize);
            for (var i = 0; i < BufferChunkSize; i++)
            {
                _randomBuffer[i] = Random.value;
            }
            _query = _entityManager.CreateEntityQuery(typeof(RandomValueComponent));
        }

        public void Update()
        {
            var entityCount = _query.CalculateEntityCount();
            var requiredBufferCapacity = Mathf.CeilToInt(entityCount / (float) BufferChunkSize) * BufferChunkSize;
            if (_randomBuffer.Length < requiredBufferCapacity)
            {
                var newRandomBuffer = _util.CreatePersistentArray<float>(requiredBufferCapacity);
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
                randomHandle = _entityManager.GetComponentTypeHandle<RandomValueComponent>(false),
                randomIndex = _randomIndex,
                randomValues = _randomBuffer
            };
            var handle = job.Schedule(chunks.Length, 32);
            
            handle.Complete();

            chunks.Dispose();
        }

        public void FinalizeSystem()
        {
            _randomBuffer.Dispose();
        }
    }
}