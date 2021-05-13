using SolidSpace.Entities;
using SolidSpace.Entities.ParticleEmitters;
using SolidSpace.Entities.Randomization;
using SolidSpace.Entities.RepeatTimer;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground
{
    public class EmitterSpawnManager : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;

        private readonly EmitterSpawnManagerConfig _config;
        private readonly IEntityManager _entityManager;

        public EmitterSpawnManager(EmitterSpawnManagerConfig config, IEntityManager entityManager)
        {
            _config = config;
            _entityManager = entityManager;
        }
        
        public void InitializeController()
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

        public void UpdateController()
        {
            
        }

        public void FinalizeController()
        {
            
        }
    }
}