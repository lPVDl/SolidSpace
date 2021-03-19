using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Despawn;
using SpaceSimulator.Runtime.Entities.Particles.Rendering;
using SpaceSimulator.Runtime.Entities.Physics;
using SpaceSimulator.Runtime.Entities.Randomization;
using SpaceSimulator.Runtime.Entities.RepeatTimer;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class TriangleParticleEmitterSystem : SystemBase
    {
        private const int BufferChunkSize = 128;
        
        private EntityArchetype _particleArchetype;
        private EntityQuery _query;
        
        private NativeArray<EmitParticleData> _resultBuffer;
        private int _entityCount;

        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        private EntityCommandBuffer _commandBuffer;
        private bool _createdCommandBuffer;

        protected override void OnStartRunning()
        {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _query = EntityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(TriangleParticleEmitterComponent),
                typeof(PositionComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent)
            });

            
            _resultBuffer = new NativeArray<EmitParticleData>(BufferChunkSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            _particleArchetype = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(DespawnComponent),
                typeof(TriangleParticleRenderComponent)
            });
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("EmitTriangleParticlesJob");
            _entityCount = _query.CalculateEntityCount();
            var requiredBufferCapacity = Mathf.CeilToInt(_entityCount / (float) BufferChunkSize) * BufferChunkSize;
            if (_resultBuffer.Length < requiredBufferCapacity)
            {
                _resultBuffer.Dispose();
                _resultBuffer = new NativeArray<EmitParticleData>(requiredBufferCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }

            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);

            var job = new EmitTriangleParticlesJob
            {
                chunks = chunks,
                timerHandle = GetComponentTypeHandle<RepeatTimerComponent>(),
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = GetComponentTypeHandle<RandomValueComponent>(true),
                time = (float)Time.ElapsedTime,
                results = _resultBuffer
            };
            var handle = job.Schedule(chunks.Length, 32, Dependency);
            
            handle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Command buffer");
            _createdCommandBuffer = false;
            var emitCount = 0;
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

                emitCount++;
                var entity = _commandBuffer.CreateEntity(_particleArchetype);
                _commandBuffer.SetComponent(entity, new PositionComponent
                {
                    value = emitData.position
                });
                _commandBuffer.SetComponent(entity, new VelocityComponent
                {
                    value = emitData.velocity
                });
                _commandBuffer.SetComponent(entity, new DespawnComponent
                {
                    despawnTime = emitData.despawnTime
                });
            }
            Profiler.EndSample();
            
            SpaceDebug.LogState("EmittedCount", emitCount);
        }

        protected override void OnDestroy()
        {
            _resultBuffer.Dispose();
        }
    }
}