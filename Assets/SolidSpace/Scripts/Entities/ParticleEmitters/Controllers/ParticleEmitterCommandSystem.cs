using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.ParticleEmitters
{
    internal class ParticleEmitterCommandSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IParticleEmitterComputeSystem _computeSystem;
        
        private EntityArchetype _particleArchetype;

        public ParticleEmitterCommandSystem(IEntityWorldManager entityManager, IParticleEmitterComputeSystem computeSystem)
        {
            _entityManager = entityManager;
            _computeSystem = computeSystem;
        }

        public void InitializeController()
        {
            _particleArchetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                // TODO : Fix reference!
                // typeof(VelocityComponent),
                typeof(DespawnComponent),
                // TODO : Fix reference!
                // typeof(RaycastComponent),
                // TODO : Fix reference!
                // typeof(PixelRenderComponent)
            });
        }

        public void UpdateController()
        {
            var entityCount = _computeSystem.ParticleCount;
            var particles = _computeSystem.Particles;
            var entities = _entityManager.CreateEntity(_particleArchetype, entityCount, Allocator.Temp);
            
            for (var i = 0; i < entityCount; i++)
            {
                var entity = entities[i];
                var particle = particles[i];
                _entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = particle.position
                });
                // TODO : Fix reference!
                // _entityManager.SetComponentData(entity, new VelocityComponent
                // {
                //     value = particle.velocity
                // });
                _entityManager.SetComponentData(entity, new DespawnComponent
                {
                    despawnTime = particle.despawnTime
                });
            }
            
            SpaceDebug.LogState("EmittedCount", entityCount);
        }

        public void FinalizeController()
        {
            
        }
    }
}