using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Sandbox.Core;
using SolidSpace.Playground.UI;
using Unity.Entities;
using Unity.Mathematics;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground.Sandbox.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly ShipSpawnToolConfig _config;
        private readonly IEntityWorldManager _entityManager;
        private readonly IUIManager _uiManager;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IPointerTracker _pointer;
        private EntityArchetype _shipArchetype;

        public ShipSpawnTool(ShipSpawnToolConfig config, IEntityWorldManager entityManager, IUIManager uiManager,
            ISpriteColorSystem spriteSystem, IHealthAtlasSystem healthSystem, IPointerTracker pointer)
        {
            _config = config;
            _entityManager = entityManager;
            _uiManager = uiManager;
            _spriteSystem = spriteSystem;
            _healthSystem = healthSystem;
            _pointer = pointer;
        }

        public void InitializeTool()
        {
            Config = _config.ToolConfig;
            
            _shipArchetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(SizeComponent),
                typeof(ColliderComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent)
            });
        }
        
        public void OnToolActivation()
        {
            
        }
        
        public void Update()
        {
            if (_uiManager.IsMouseOver || !_pointer.ClickedThisFrame)
            {
                return;
            }
            
            var texture = _config.ShipTexture;
            var size = new int2(texture.width, texture.height);
            var colorIndex = _spriteSystem.Allocate(size.x, size.y);
            var healthIndex = _healthSystem.Allocate(size.x * size.y);
            
            var entity = _entityManager.CreateEntity(_shipArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = _pointer.Position
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

        public void OnToolDeactivation()
        {
            
        }

        public void FinalizeTool()
        {
            
        }
    }
}