using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Sandbox.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground.Sandbox.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool
    {
        public Sprite Icon => _config.ToolIcon;
        
        private readonly ShipSpawnToolConfig _config;
        private readonly IEntityWorldManager _entityManager;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private ComponentType[] _shipArchetype;

        public ShipSpawnTool(ShipSpawnToolConfig config, IEntityWorldManager entityManager,
            ISpriteColorSystem spriteSystem, IHealthAtlasSystem healthSystem)
        {
            _config = config;
            _entityManager = entityManager;
            _spriteSystem = spriteSystem;
            _healthSystem = healthSystem;
        }
        
        public void Initialize()
        {
            _shipArchetype = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(SizeComponent),
                typeof(ColliderComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent)
            };
        }
        
        public void OnMouseClick(float2 clickPosition)
        {
            var texture = _config.ShipTexture;
            var size = new int2(texture.width, texture.height);
            var colorIndex = _spriteSystem.Allocate(size.x, size.y);
            var healthIndex = _healthSystem.Allocate(size.x * size.y);
            
            var entity = _entityManager.CreateEntity(_shipArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = clickPosition
            });
            _entityManager.SetComponentData(entity, new SizeComponent
            {
                value = new half2((half)size.x, (half)size.y)
            });
            _entityManager.SetComponentData(entity, new RotationComponent
            {
                value = (half) Random.value
            });
            _entityManager.SetComponentData(entity, new SpriteRenderComponent
            {
                index = colorIndex
            });
            _entityManager.SetComponentData(entity, new HealthComponent
            {
                index = healthIndex
            });
            
            _spriteSystem.Copy(texture, colorIndex);
            _healthSystem.Copy(texture, healthIndex);
        }
        
        public void OnToolSelected()
        {
            
        }

        public void OnToolDeselected()
        {
            
        }

        public void FinalizeTool()
        {
            
        }
    }
}