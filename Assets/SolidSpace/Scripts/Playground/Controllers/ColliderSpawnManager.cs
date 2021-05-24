using System.Collections.Generic;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
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
            public float2 position;
            public float rotation;
            public float2 size;
            public Entity entity;
        }

        public EControllerType ControllerType => EControllerType.Playground;

        private readonly IEntityWorldManager _entityManager;
        private readonly ColliderSpawnManagerConfig _config;
        private readonly Camera _camera;
        private readonly IGizmosManager _gizmosManager;
        private readonly List<ColliderInfo> _spawnedColliders;
        private readonly ComponentType[] _colliderArchetype;

        private GizmosHandle _gizmos;

        public ColliderSpawnManager(IEntityWorldManager entityManager, ColliderSpawnManagerConfig config, Camera camera,
            IGizmosManager gizmosManager)
        {
            _entityManager = entityManager;
            _config = config;
            _camera = camera;
            _gizmosManager = gizmosManager;
            _spawnedColliders = new List<ColliderInfo>();
            _colliderArchetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ColliderComponent),
                typeof(RotationComponent),
                typeof(SizeComponent)
            };
        }
        
        public void InitializeController()
        {
            _gizmos = _gizmosManager.GetHandle(this);
            
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

            var color = _config.GizmosColor;
            foreach (var info in _spawnedColliders)
            {
                _gizmos.DrawWireRect(info.position, info.size, info.rotation, color);
            }
        }

        private void DestroyNearest(float2 position)
        {
            if (_spawnedColliders.Count == 0)
            {
                return;
            }
            
            var minIndex = -1;
            var minValue = float.MaxValue;
            for (var i = 0; i < _spawnedColliders.Count; i++)
            {
                var distance = FloatMath.Distance(_spawnedColliders[i].position, position);
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

        private void SpawnCollider(float2 position)
        {
            var width = (half) Random.Range(_config.ColliderWidth.x, _config.ColliderWidth.y);
            var height = (half) Random.Range(_config.ColliderHeight.x, _config.ColliderHeight.y);
            var rotation = Random.value;
            var info = new ColliderInfo
            {
                position = position,
                rotation = FloatMath.TwoPI * rotation,
                entity = _entityManager.CreateEntity(_colliderArchetype),
                size = new float2(width, height)
            };
            _spawnedColliders.Add(info);
            
            _entityManager.SetComponentData(info.entity, new PositionComponent
            {
                value = new float2(position.x, position.y)
            });
            _entityManager.SetComponentData(info.entity, new SizeComponent
            {
                value = new half2(width, height)
            });
            _entityManager.SetComponentData(info.entity, new RotationComponent
            {
                value = (half) rotation
            });
        }

        private bool GetClickPosition(out float2 clickPosition)
        {
            clickPosition = float2.zero;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(-Vector3.forward, Vector3.zero);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }
            
            var hitPos = ray.origin + ray.direction * distance;
            clickPosition = new float2
            {
                x = hitPos.x,
                y = hitPos.y
            };
            
            return true;
        }

        public void FinalizeController()
        {
            
        }
    }
}