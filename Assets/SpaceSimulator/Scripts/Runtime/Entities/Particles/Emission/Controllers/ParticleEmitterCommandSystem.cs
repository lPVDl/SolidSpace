using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Despawn;
using SpaceSimulator.Runtime.Entities.Particles.Rendering;
using SpaceSimulator.Runtime.Entities.Physics;
using SpaceSimulator.Runtime.Entities.Physics.Velocity;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    public class ParticleEmitterCommandSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;
        
        private readonly IEntityManager _entityManager;
        private readonly ParticleEmitterComputeSystem _computeSystem;
        
        private EntityArchetype _particleArchetype;

        public ParticleEmitterCommandSystem(IEntityManager entityManager, ParticleEmitterComputeSystem computeSystem)
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
                typeof(ParticleRenderComponent)
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