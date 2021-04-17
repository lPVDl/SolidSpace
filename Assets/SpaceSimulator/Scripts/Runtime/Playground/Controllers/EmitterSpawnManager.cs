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
        private readonly IParticleMeshSystem _renderSystem;
        private readonly IEntityManager _entityManager;

        public EmitterSpawnManager(EmitterSpawnManagerConfig config, IParticleMeshSystem renderSystem, IEntityManager entityManager)
        {
            _config = config;
            _renderSystem = renderSystem;
            _entityManager = entityManager;
        }
        
        public void Initialize()
        {
            _renderSystem.Material = _config.ParticleMaterial;

            var componentTypes = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent),
            };
            var shipArchetype = _entityManager.CreateArchetype(componentTypes);
            
            using var entityArray = _entityManager.CreateEntity(shipArchetype, _config.EntityCount, Allocator.Temp);

            foreach (var entity in entityArray)
            {
                var x = Random.Range(_config.SpawnRangeX.x, _config.SpawnRangeX.y);
                var y = Random.Range(_config.SpawnRangeY.x, _config.SpawnRangeY.y);
                _entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(x, y)
                });
                _entityManager.SetComponentData(entity, new RepeatTimerComponent
                {
                    delay = 1 / _config.SpawnRate
                });
                _entityManager.SetComponentData(entity, new ParticleEmitterComponent
                {
                    particleVelocity = _config.ParticleVelocity
                });
            }
        }
    }
}