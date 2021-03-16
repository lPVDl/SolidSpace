using SpaceMassiveSimulator.Runtime.Entities.Particles.Rendering;
using SpaceMassiveSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles.Emission
{
    public class TriangleParticleEmitterSystem : SystemBase
    {
        private const int BufferChunkSize = 128;
        
        private EntityArchetype _particleArchetype;
        private EntityQuery _emitterQuery;
        private NativeArray<float> _randomBuffer;
        private NativeArray<EmitParticleData> _resultBuffer;
        private int _randomIndex;
        private int _entityCount;

        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        private EntityCommandBuffer _commandBuffer;
        private bool _createdCommandBuffer;

        protected override void OnStartRunning()
        {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _emitterQuery = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(TriangleParticleEmitterComponent),
                typeof(PositionComponent)
            });

            _randomBuffer = new NativeArray<float>(BufferChunkSize, Allocator.Persistent);
            for (var i = 0; i < BufferChunkSize; i++)
            {
                _randomBuffer[i] = Random.value;
            }
            _resultBuffer = new NativeArray<EmitParticleData>(BufferChunkSize, Allocator.Persistent);

            _particleArchetype = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(TriangleParticleRenderComponent)
            });
        }

        protected override void OnUpdate()
        {
            _createdCommandBuffer = false;
            
            for (var i = 0; i < _entityCount; i++)
            {
                var emitData = _resultBuffer[i];
                
                if (!emitData.emit)
                {
                    continue;
                }

                if (!_createdCommandBuffer)
                {
                    _commandBuffer = _commandBufferSystem.CreateCommandBuffer();
                    _createdCommandBuffer = true;
                }

                var entity = _commandBuffer.CreateEntity(_particleArchetype);
                _commandBuffer.SetComponent(entity, new PositionComponent
                {
                    value = emitData.position
                });
                _commandBuffer.SetComponent(entity, new VelocityComponent
                {
                    value = emitData.velocity
                });
            }
            
            _entityCount = _emitterQuery.CalculateEntityCount();
            var requiredBufferCapacity = Mathf.CeilToInt(_entityCount / (float) BufferChunkSize) * BufferChunkSize;
            if (_randomBuffer.Length != requiredBufferCapacity)
            {
                var newRandomBuffer = new NativeArray<float>(requiredBufferCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                for (var i = 0; i < _resultBuffer.Length; i++)
                {
                    newRandomBuffer[i] = _randomBuffer[i];
                }
                for (var i = _randomBuffer.Length; i < newRandomBuffer.Length; i++)
                {
                    newRandomBuffer[i] = Random.value;
                }
                _randomBuffer.Dispose();
                _randomBuffer = newRandomBuffer;
                _resultBuffer.Dispose();
                _resultBuffer = new NativeArray<EmitParticleData>(requiredBufferCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }
            
            _randomIndex = (_randomIndex + 1) % _randomBuffer.Length;
            _randomBuffer[_randomIndex] = Random.value;
            
            var chunks = _emitterQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            var job = new EmitTriangleParticlesJob
            {
                deltaTime = Time.DeltaTime,
                chunks = chunks,
                randomIndex = _randomIndex,
                emitterHandle = GetComponentTypeHandle<TriangleParticleEmitterComponent>(),
                positionHandle = GetComponentTypeHandle<PositionComponent>(),
                randomValues = _randomBuffer,
                result = _resultBuffer
            };
            
            Dependency = job.Schedule(chunks.Length, 32, Dependency);
        }

        protected override void OnDestroy()
        {
            _randomBuffer.Dispose();
            _resultBuffer.Dispose();
        }
    }
}