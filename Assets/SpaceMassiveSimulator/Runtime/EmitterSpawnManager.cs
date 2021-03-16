using SpaceMassiveSimulator.Runtime.Entities.Particles;
using SpaceMassiveSimulator.Runtime.Entities.Particles.Emission;
using SpaceMassiveSimulator.Runtime.Entities.Particles.Rendering;
using SpaceMassiveSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceMassiveSimulator.Runtime
{
    public class EmitterSpawnManager : MonoBehaviour
    {
        [SerializeField] private int _enitityCount;
        [SerializeField] private Material _particleMaterial;
        [SerializeField] private float _spawnRate;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            var entityManager = world.EntityManager;

            var meshBatchSystem = world.GetOrCreateSystem<TriangleParticleRenderSystem>();
            meshBatchSystem.Material = _particleMaterial;

            var componentTypes = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(TriangleParticleEmitterComponent),
            };
            var shipArchetype = entityManager.CreateArchetype(componentTypes);

            using var entityArray = new NativeArray<Entity>(_enitityCount, Allocator.Temp);

            entityManager.CreateEntity(shipArchetype, entityArray);

            foreach (var entity in entityArray)
            {
                entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(Random.Range(-10f, 10f), Random.Range(-10f, 10f))
                });
                entityManager.SetComponentData(entity, new TriangleParticleEmitterComponent
                {
                    seed = UnityEngine.Random.value,
                    spawnDelay = 1 / _spawnRate
                });
            }
        }
    }
}