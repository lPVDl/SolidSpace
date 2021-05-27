using System.Collections.Generic;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground
{
    public class ShipSpawnManager : IController
    {
        private struct ShipInfo
        {
            public float2 position;
            public Entity entity;
            public AtlasIndex colorIndex;
            public AtlasIndex healthIndex;
        }
        
        public EControllerType ControllerType => EControllerType.Playground;

        private readonly Camera _camera;
        private readonly ShipSpawnManagerConfig _config;
        private readonly IEntityWorldManager _entityManager;
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly ComponentType[] _archetype;
        private readonly List<ShipInfo> _spawnedEntities;

        public ShipSpawnManager(Camera camera, ShipSpawnManagerConfig config, IEntityWorldManager entityManager, 
            ISpriteColorSystem colorSystem, IHealthAtlasSystem healthSystem)
        {
            _camera = camera;
            _config = config;
            _entityManager = entityManager;
            _colorSystem = colorSystem;
            _healthSystem = healthSystem;
            _spawnedEntities = new List<ShipInfo>();
            _archetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(SizeComponent),
                typeof(ColliderComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent)
            };
        }
        
        public void InitializeController()
        {
            
        }

        public void UpdateController()
        {
            if (!_config.MouseControl)
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0) && GetClickPosition(out var clickPosition))
            {
                Spawn(clickPosition);
            }

            if (Input.GetMouseButtonDown(1) && GetClickPosition(out clickPosition))
            {
                Destroy(clickPosition);
            }
        }

        private void Spawn(float2 position)
        {
            var texture = _config.ShipTexture;
            var size = new int2(texture.width, texture.height);
            var info = new ShipInfo
            {
                position = position,
                entity = _entityManager.CreateEntity(_archetype),
                colorIndex = _colorSystem.Allocate(size.x, size.y),
                healthIndex = _healthSystem.Allocate(size.x * size.y)
            };
            _spawnedEntities.Add(info);
            
            _entityManager.SetComponentData(info.entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(info.entity, new SizeComponent
            {
                value = new half2((half)size.x, (half)size.y)
            });
            _entityManager.SetComponentData(info.entity, new RotationComponent
            {
                value = (half) Random.value
            });
            _entityManager.SetComponentData(info.entity, new SpriteRenderComponent
            {
                index = info.colorIndex
            });
            _entityManager.SetComponentData(info.entity, new HealthComponent
            {
                index = info.healthIndex
            });
            
            _colorSystem.Copy(texture, info.colorIndex);
            _healthSystem.Copy(texture, info.healthIndex);
        }

        private void Destroy(float2 position)
        {
            if (_spawnedEntities.Count == 0)
            {
                return;
            }

            var minIndex = -1;
            var minValue = float.MaxValue;
            for (var i = 0; i < _spawnedEntities.Count; i++)
            {
                var distance = FloatMath.Distance(_spawnedEntities[i].position, position);
                if (distance < minValue)
                {
                    minValue = distance;
                    minIndex = i;
                }
            }

            var info = _spawnedEntities[minIndex];
            _colorSystem.Release(info.colorIndex);
            _healthSystem.Release(info.healthIndex);
            _spawnedEntities.RemoveAt(minIndex);
            _entityManager.DestroyEntity(info.entity);
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