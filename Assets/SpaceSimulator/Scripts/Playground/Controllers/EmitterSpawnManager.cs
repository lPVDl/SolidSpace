using SpaceSimulator.Entities;
using SpaceSimulator.Entities.EntityWorld;
using SpaceSimulator.Entities.Particles.Emission;
using SpaceSimulator.Entities.Randomization;
using SpaceSimulator.Entities.RepeatTimer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace SpaceSimulator.Playground
{
    public class EmitterSpawnManager : IInitializable
    {
        public EControllerType ControllerType => EControllerType.Playground;

        private readonly EmitterSpawnManagerConfig _config;
        private readonly IEntityManager _entityManager;

        public EmitterSpawnManager(EmitterSpawnManagerConfig config, IEntityManager entityManager)
        {
            _config = config;
            _entityManager = entityManager;
        }
        
        public void Initialize()
        {
            var archetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent),
            });
            
            using var entityArray = _entityManager.CreateEntity(archetype, _config.EntityCount, Allocator.Temp);

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