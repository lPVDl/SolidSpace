using System.Collections.Generic;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground
{
    public class ColliderSpawnManager : IController
    {
        private struct ColliderInfo
        {
            public Vector3 position;
            public Entity entity;
        }

        public EControllerType ControllerType => EControllerType.Playground;

        private readonly IEntityWorldManager _entityManager;
        private readonly ColliderSpawnManagerConfig _config;
        private readonly Camera _camera;
        private readonly List<ColliderInfo> _spawnedColliders;
        private readonly ComponentType[] _colliderArchetype;

        public ColliderSpawnManager(IEntityWorldManager entityManager, ColliderSpawnManagerConfig config, Camera camera)
        {
            _entityManager = entityManager;
            _config = config;
            _camera = camera;
            _spawnedColliders = new List<ColliderInfo>();
            _colliderArchetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent)
            };
        }
        
        public void InitializeController()
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

        public void UpdateController()
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
            _entityManager.DestroyEntity(entity);
        }

        private void SpawnCollider(Vector3 position)
        {
            var info = new ColliderInfo
            {
                position = position,
                entity = _entityManager.CreateEntity(_colliderArchetype)
            };
            _spawnedColliders.Add(info);
            
            _entityManager.SetComponentData(info.entity, new PositionComponent
            {
                value = new float2(position.x, position.y)
            });
            
            _entityManager.SetComponentData(info.entity, new ColliderComponent
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

        public void FinalizeController()
        {
            
        }
    }
}