using SolidSpace.Entities;
using SolidSpace.Entities.Physics;
using SolidSpace.Entities.Rendering.Pixels;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace SolidSpace.Playground
{
    public class ParticleSpawnManager : IInitializable
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly ParticleSpawnManagerConfig _config;
        private readonly IEntityManager _entityManager;

        public ParticleSpawnManager(ParticleSpawnManagerConfig config, IEntityManager entityManager)
        {
            _config = config;
            _entityManager = entityManager;
        }
        
        public void Initialize()
        {
            var archetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(PixelRenderComponent),
            });

            using var entityArray = _entityManager.CreateEntity(archetype, _config.SpawnCount, Allocator.Temp);

            foreach (var entity in entityArray)
            {
                var posX = Random.Range(_config.SpawnRangeX.x, _config.SpawnRangeX.y);
                var posY = Random.Range(_config.SpawnRangeY.x, _config.SpawnRangeY.y);
                var velocity = Random.insideUnitSphere * _config.Velocity;
                
                _entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(posX, posY)
                });
                
                _entityManager.SetComponentData(entity, new VelocityComponent
                {
                    value = new float2(velocity.x, velocity.y)
                });
            }
        }
    }
}