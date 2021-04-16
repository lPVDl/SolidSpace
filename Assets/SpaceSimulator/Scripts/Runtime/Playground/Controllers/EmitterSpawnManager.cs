using SpaceSimulator.Runtime.Entities;
using SpaceSimulator.Runtime.Entities.Particles.Emission;
using SpaceSimulator.Runtime.Entities.Particles.Rendering;
using SpaceSimulator.Runtime.Entities.Randomization;
using SpaceSimulator.Runtime.Entities.RepeatTimer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Random = UnityEngine.Random;

namespace SpaceSimulator.Runtime.Playground
{
    public class EmitterSpawnManager : IInitializable
    {
        public EControllerType ControllerType => EControllerType.Common;

        private readonly EmitterSpawnManagerConfig _config;
        private readonly ParticleMeshSystem _renderSystem;
        private readonly IEntityWorld _world;

        public EmitterSpawnManager(EmitterSpawnManagerConfig config, ParticleMeshSystem renderSystem, IEntityWorld world)
        {
            _config = config;
            _renderSystem = renderSystem;
            _world = world;
        }
        
        public void Initialize()
        {
            var entityManager = _world.EntityManager;
            _renderSystem.Material = _config.ParticleMaterial;

            var componentTypes = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent),
            };
            var shipArchetype = entityManager.CreateArchetype(componentTypes);

            using var entityArray = new NativeArray<Entity>(_config.EntityCount, Allocator.Temp);

            entityManager.CreateEntity(shipArchetype, entityArray);

            foreach (var entity in entityArray)
            {
                var x = Random.Range(_config.SpawnRangeX.x, _config.SpawnRangeX.y);
                var y = Random.Range(_config.SpawnRangeY.x, _config.SpawnRangeY.y);
                entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(x, y)
                });
                entityManager.SetComponentData(entity, new RepeatTimerComponent
                {
                    delay = 1 / _config.SpawnRate
                });
                entityManager.SetComponentData(entity, new ParticleEmitterComponent
                {
                    particleVelocity = _config.ParticleVelocity
                });
            }
        }
    }
}