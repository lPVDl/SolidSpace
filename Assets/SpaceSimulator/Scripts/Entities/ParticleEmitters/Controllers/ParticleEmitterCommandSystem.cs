using SpaceSimulator.DebugUtils;
using SpaceSimulator.Entities.Despawn;
using SpaceSimulator.Entities.Physics;
using SpaceSimulator.Entities.Rendering.Pixels;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Entities.ParticleEmitters
{
    public class ParticleEmitterCommandSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;
        
        private readonly IEntityManager _entityManager;
        private readonly IParticleEmitterComputeSystem _computeSystem;
        
        private EntityArchetype _particleArchetype;

        public ParticleEmitterCommandSystem(IEntityManager entityManager, IParticleEmitterComputeSystem computeSystem)
        {
            _entityManager = entityManager;
            _computeSystem = computeSystem;
        }

        public void Initialize()
        {
            _particleArchetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(DespawnComponent),
                typeof(RaycastComponent),
                typeof(PixelRenderComponent)
            });
        }

        public void Update()
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
                _entityManager.SetComponentData(entity, new VelocityComponent
                {
                    value = particle.velocity
                });
                _entityManager.SetComponentData(entity, new DespawnComponent
                {
                    despawnTime = particle.despawnTime
                });
            }
            
            SpaceDebug.LogState("EmittedCount", entityCount);
        }

        public void FinalizeSystem()
        {
            
        }
    }
}