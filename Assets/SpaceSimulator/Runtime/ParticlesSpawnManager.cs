using SpaceSimulator.Runtime.Entities.Particles.Rendering;
using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceSimulator.Runtime
{
    public class ParticlesSpawnManager : MonoBehaviour
    {
        [SerializeField] private int _enitityCount;
        [SerializeField] private Material _particleMaterial;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            var entityManager = world.EntityManager;

            var renderSystem = world.GetOrCreateSystem<TriangleParticleMeshRenderSystem>();
            renderSystem.Material = _particleMaterial;

            var componentTypes = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(TriangleParticleRenderComponent),
            };
            var archetype = entityManager.CreateArchetype(componentTypes);

            using var entityArray = new NativeArray<Entity>(_enitityCount, Allocator.Temp);

            entityManager.CreateEntity(archetype, entityArray);

            foreach (var entity in entityArray)
            {
                entityManager.SetComponentData(entity, new VelocityComponent
                {
                    value = new float2(0, Random.Range(1f, 2f))
                });

                entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(Random.Range(-10f, 10f), Random.Range(-10f, 10f))
                });
            }
        }
    }
}