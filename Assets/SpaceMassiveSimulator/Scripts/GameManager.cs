using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

        private ShipMeshBatchSystem _meshBatchSystem;
        
        private List<MeshFilter> _meshFilters = new List<MeshFilter>();
        
        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            
            var entityManager = world.EntityManager;

            _meshBatchSystem = world.GetOrCreateSystem(typeof(ShipMeshBatchSystem)) as ShipMeshBatchSystem;

            for (var i = 0; i < 64; i++)
            {
                var fab = GameObject.Instantiate(_meshFilterPrefab, transform);
                _meshFilters.Add(fab.GetComponent<MeshFilter>());
            }

            var componentTypes = new ComponentType[]
            {
                typeof(ShipData),
                typeof(Translation),
                //typeof(RenderMesh),
                //typeof(RenderBounds),
                //typeof(LocalToWorld),
                typeof(ShipMovementData)
            };
            var shipArchetype = entityManager.CreateArchetype(componentTypes);

            using var entityArray = new NativeArray<Entity>(_enitityCount, Allocator.Temp);
            
            entityManager.CreateEntity(shipArchetype, entityArray);

            foreach (var entity in entityArray)
            {
                entityManager.SetComponentData(entity, new ShipData
                {
                    health = Random.Range(1, 100),
                    maxHealth = Random.Range(1000, 10000),
                    healthRegen = Random.Range(1, 10),
                });
                
                // entityManager.SetSharedComponentData(entity, new RenderMesh
                // {
                //     mesh = _shipMesh,
                //     material = _shipMaterials[Random.Range(0, _shipMaterials.Length)],
                // });
                
                entityManager.SetComponentData(entity, new ShipMovementData()
                {
                    movementSpeed = Random.Range(1f, 2f),
                });
                
                entityManager.SetComponentData(entity, new Translation
                {
                    Value = new float3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0)
                });
            }
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _meshBatchSystem.Meshes.Count; i++)
            {
                _meshFilters[i].mesh = _meshBatchSystem.Meshes[i];
            }
        }
    }
}
