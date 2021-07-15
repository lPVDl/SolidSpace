using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.Spawn;
using SolidSpace.UI.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool, ISpawnToolHandler
    {
        private readonly ShipSpawnToolConfig _config;
        private readonly IEntityManager _entityManager;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly ISpawnToolFactory _spawnToolFactory;
        private readonly IPlaygroundUIManager _uiManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IHealthAtlasSystem _healthSystem;

        private ISpawnTool _spawnTool;
        private EntityArchetype _shipArchetype;
        private IUIElement _componentsWindow;
        private GizmosHandle _gizmos;
        private float2 _textureSize;

        public ShipSpawnTool(ShipSpawnToolConfig config, IEntityManager entityManager, IHealthAtlasSystem healthSystem,
            ISpriteColorSystem spriteSystem, ISpawnToolFactory spawnToolFactory, IPlaygroundUIManager uiManager,
            IComponentFilterFactory filterFactory, IGizmosManager gizmosManager)
        {
            _config = config;
            _entityManager = entityManager;
            _spriteSystem = spriteSystem;
            _spawnToolFactory = spawnToolFactory;
            _uiManager = uiManager;
            _filterFactory = filterFactory;
            _gizmosManager = gizmosManager;
            _healthSystem = healthSystem;
        }

        public void OnInitialize()
        {
            var shipComponents = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(RectSizeComponent),
                typeof(RectColliderComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent),
                typeof(VelocityComponent),
                typeof(ActorComponent),
                typeof(RigidbodyComponent)
            };
            _textureSize = new float2(_config.ShipTexture.width, _config.ShipTexture.height);
            _shipArchetype = _entityManager.CreateArchetype(shipComponents);
            _componentsWindow = _filterFactory.CreateReadonly(shipComponents);
            _spawnTool = _spawnToolFactory.Create(this);
            _gizmos = _gizmosManager.GetHandle(this, Color.yellow);
        }
        
        public void OnActivate(bool isActive)
        {
            _spawnTool.OnActivate(isActive);       
            _uiManager.SetElementVisible(_componentsWindow, isActive);
        }
        
        public void OnUpdate()
        {
            _spawnTool.OnUpdate();
        }
        
        public void OnSpawnEvent(SpawnEventData eventData)
        {
            var origin = eventData.origin;
            
            switch (eventData.eventType)
            {
                case ESpawnEventType.Preview:
                    _gizmos.DrawWireRect(origin.position, _textureSize, origin.rotation);
                    var xAxis = FloatMath.Rotate(_textureSize.x * 0.5f, origin.rotation);
                    _gizmos.DrawLine(origin.position, origin.position + xAxis);
                    break;
                
                case ESpawnEventType.Place:
                    if (_spriteSystem.AllocatedIndexCount < 1024)
                    {
                        SpawnShip(origin.position, origin.rotation);
                    }
   
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSpawnCircle(float2 position, float radius)
        {
            _gizmos.DrawScreenCircle(position, radius);
        }

        private void SpawnShip(float2 position, float rotation)
        {
            var texture = _config.ShipTexture;
            var size = new int2(texture.width, texture.height);
            var colorIndex = _spriteSystem.Allocate(size.x, size.y);
            var healthIndex = _healthSystem.Allocate(size.x * size.y);
            
            var entity = _entityManager.CreateEntity(_shipArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RectSizeComponent
            {
                value = new half2((half)size.x, (half)size.y)
            });
            _entityManager.SetComponentData(entity, new RotationComponent
            {
                value = rotation
            });
            _entityManager.SetComponentData(entity, new SpriteRenderComponent
            {
                index = colorIndex
            });
            _entityManager.SetComponentData(entity, new HealthComponent
            {
                index = healthIndex
            });
            _entityManager.SetComponentData(entity, new ActorComponent
            {
                isActive = false
            });
            
            _spriteSystem.Copy(texture, colorIndex);
            _healthSystem.Copy(texture, healthIndex);
        }

        public void OnFinalize() { }
    }
}