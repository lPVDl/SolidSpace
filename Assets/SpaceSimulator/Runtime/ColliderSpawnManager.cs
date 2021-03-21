using System.Collections.Generic;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Physics.Collision;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceSimulator.Runtime
{
    public class ColliderSpawnManager : MonoBehaviour
    {
        private struct ColliderInfo
        {
            public Vector3 position;
            public Entity entity;
        }
        
        [SerializeField] private Camera _camera;
        [SerializeField] private float _colliderRadius;
        [SerializeField] private int _onStartSpawnCount;
        [SerializeField] private int _spawnPerSpawn;
        [SerializeField] private float _spawnExtraRadius;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;

        private List<ColliderInfo> _spawnedColliders;
        private ComponentType[] _colliderArchetype;

        private void Awake()
        {
            _spawnedColliders = new List<ColliderInfo>();
            _colliderArchetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            };
        }

        private void Start()
        {
            for (var i = 0; i < _onStartSpawnCount; i++)
            {
                var x = Random.Range(_spawnRangeX.x, _spawnRangeX.y);
                var y = Random.Range(_spawnRangeY.x, _spawnRangeY.y);
                var samplePosition = new Vector2(x, y);
                
                for (var j = 0; j < _spawnPerSpawn; j++)
                {
                    var finalPosition = samplePosition + Random.insideUnitCircle * _spawnExtraRadius;
                    SpawnCollider(finalPosition);
                }
                
                
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && GetClickPosition(out var clickPosition))
            {
                SpawnCollider(clickPosition);
            }

            if (Input.GetMouseButtonDown(1) && GetClickPosition(out clickPosition))
            {
                DestroyNearest(clickPosition);
            }
        }

        private void DestroyNearest(Vector3 position)
        {
            if (_spawnedColliders.Count == 0)
            {
                return;
            }
            
            var minIndex = -1;
            var minValue = float.MaxValue;
            for (var i = 0; i < _spawnedColliders.Count; i++)
            {
                var distance = (_spawnedColliders[i].position - position).magnitude;
                if (distance < minValue)
                {
                    minValue = distance;
                    minIndex = i;
                }
            }

            var entity = _spawnedColliders[minIndex].entity;
            _spawnedColliders.RemoveAt(minIndex);
            
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            entityManager.DestroyEntity(entity);
        }

        private void SpawnCollider(Vector3 position)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            
            var info = new ColliderInfo
            {
                position = position,
                entity = entityManager.CreateEntity(_colliderArchetype)
            };
            _spawnedColliders.Add(info);
            
            entityManager.SetComponentData(info.entity, new PositionComponent
            {
                value = new float2(position.x, position.y)
            });
            
            entityManager.SetComponentData(info.entity, new ColliderComponent
            {
                radius = _colliderRadius
            });
        }

        private bool GetClickPosition(out Vector3 clickPosition)
        {
            clickPosition = Vector3.zero;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(-Vector3.forward, Vector3.zero);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }
            
            clickPosition = ray.origin + ray.direction * distance;
            
            return true;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            foreach (var colliderInfo in _spawnedColliders)
            {
                Gizmos.DrawWireSphere(colliderInfo.position, _colliderRadius);
            }
        }
    }
}