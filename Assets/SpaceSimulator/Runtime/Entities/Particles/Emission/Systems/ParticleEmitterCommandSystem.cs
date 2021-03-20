using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Despawn;
using SpaceSimulator.Runtime.Entities.Particles.Rendering;
using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = false, OrderLast = true)]
    public class ParticleEmitterCommandSystem : SystemBase
    {
        private ParticleEmitterComputeSystem _computeSystem;
        private EntityArchetype _particleArchetype;
        
        protected override void OnStartRunning()
        {
            _computeSystem = World.GetOrCreateSystem<ParticleEmitterComputeSystem>();
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
            if (!_computeSystem.Enabled)
            {
                return;
            }

            var entityCount = _computeSystem.ParticleCount;
            var particles = _computeSystem.Particles;
            var entities = EntityManager.CreateEntity(_particleArchetype, entityCount, Allocator.Temp);
            for (var i = 0; i < entityCount; i++)
            {
                var entity = entities[i];
                var particle = particles[i];
                EntityManager.SetComponentData(entity, new PositionComponent
                {
                    value = particle.position
                });
                EntityManager.SetComponentData(entity, new VelocityComponent
                {
                    value = particle.velocity
                });
                EntityManager.SetComponentData(entity, new DespawnComponent
                {
                    despawnTime = particle.despawnTime
                });
            }
            
            SpaceDebug.LogState("EmittedCount", entityCount);
        }
    }
}