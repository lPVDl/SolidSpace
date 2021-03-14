using System.Collections.Generic;
using SpaceMassiveSimulator.Runtime.Entities.Particles;
using SpaceMassiveSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace ECSTest.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Mesh _shipMesh;
        [SerializeField] private Material[] _shipMaterials;
        [SerializeField] private int _enitityCount;
        [SerializeField] private GameObject _meshFilterPrefab;
        [SerializeField] private Material _particleMaterial;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            
            var entityManager = world.EntityManager;

            var meshBatchSystem = world.GetOrCreateSystem<TriangleParticleRenderSystem>();
            meshBatchSystem.Material = _particleMaterial;

            var componentTypes = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent)
            };
            var shipArchetype = entityManager.CreateArchetype(componentTypes);

            using var entityArray = new NativeArray<Entity>(_enitityCount, Allocator.Temp);
            
            entityManager.CreateEntity(shipArchetype, entityArray);

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
