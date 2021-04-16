using System.Collections.Generic;
using SpaceSimulator.Runtime.Entities;
using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceSimulator.Runtime.Playground
{
    public class ColliderSpawnManager : IInitializable, IUpdatable
    {
        private struct ColliderInfo
        {
            public Vector3 position;
            public Entity entity;
        }

        public EControllerType ControllerType => EControllerType.Common;

        private readonly IEntityWorld _world;
        private readonly ColliderSpawnManagerConfig _config;
        private readonly Camera _camera;
        private readonly List<ColliderInfo> _spawnedColliders;
        private readonly ComponentType[] _colliderArchetype;

        public ColliderSpawnManager(IEntityWorld world, ColliderSpawnManagerConfig config, Camera camera)
        {
            _world = world;
            _config = config;
            _camera = camera;
            _spawnedColliders = new List<ColliderInfo>();
            _colliderArchetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            };
        }
        
        public void Initialize()
        {
            for (var i = 0; i < _config.OnStartSpawnCount; i++)
            {
                var x = Random.Range(_config.SpawnRangeX.x, _config.SpawnRangeX.y);
                var y = Random.Range(_config.SpawnRangeY.x, _config.SpawnRangeY.y);
                var samplePosition = new Vector2(x, y);
                
                for (var j = 0; j < _config.SpawnPerSpawn; j++)
                {
                    var finalPosition = samplePosition + Random.insideUnitCircle * _config.SpawnExtraRadius;
                    SpawnCollider(finalPosition);
                }
            }
        }

        public void Update()
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
            var entityManager = _world.EntityManager;
            
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
                radius = _config.ColliderRadius
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
            if (!_config.DrawGizmos)
            {
                return;
            }
            
            foreach (var colliderInfo in _spawnedColliders)
            {
                Gizmos.DrawWireSphere(colliderInfo.position, _config.ColliderRadius);
            }
        }
    }
}