using SpaceSimulator.Runtime.Entities.Particles.Emission;
using SpaceSimulator.Runtime.Entities.Particles.Rendering;
using SpaceSimulator.Runtime.Entities.Physics;
using SpaceSimulator.Runtime.Entities.Randomization;
using SpaceSimulator.Runtime.Entities.RepeatTimer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceSimulator.Runtime
{
    public class EmitterSpawnManager : MonoBehaviour
    {
        [SerializeField] private int _enitityCount;
        [SerializeField] private Material _particleMaterial;
        [SerializeField] private float _spawnRate;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            var entityManager = world.EntityManager;

            var renderSystem = world.GetOrCreateSystem<ParticleMeshRenderSystem>();
            renderSystem.Material = _particleMaterial;

            var componentTypes = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(TriangleParticleEmitterComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent),
            };
            var shipArchetype = entityManager.CreateArchetype(componentTypes);

            using var entityArray = new NativeArray<Entity>(_enitityCount, Allocator.Temp);

            entityManager.CreateEntity(shipArchetype, entityArray);

            foreach (var entity in entityArray)
            {
                var x = Random.Range(_spawnRangeX.x, _spawnRangeX.y);
                var y = Random.Range(_spawnRangeY.x, _spawnRangeY.y);
                entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(x, y)
                });
                entityManager.SetComponentData(entity, new RepeatTimerComponent
                {
                    delay = 1 / _spawnRate
                });
            }
        }
    }
}