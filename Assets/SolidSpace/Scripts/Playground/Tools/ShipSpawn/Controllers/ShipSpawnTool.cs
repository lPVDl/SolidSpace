using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.SpawnPoint;
using Unity.Entities;
using Unity.Mathematics;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly ShipSpawnToolConfig _config;
        private readonly IEntityWorldManager _entityManager;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly ISpawnPointToolFactory _pointToolFactory;
        private readonly IHealthAtlasSystem _healthSystem;

        private ISpawnPointTool _spawnPointTool;
        private EntityArchetype _shipArchetype;

        public ShipSpawnTool(ShipSpawnToolConfig config, IEntityWorldManager entityManager, IHealthAtlasSystem healthSystem,
            ISpriteColorSystem spriteSystem, ISpawnPointToolFactory pointToolFactory)
        {
            _config = config;
            _entityManager = entityManager;
            _spriteSystem = spriteSystem;
            _pointToolFactory = pointToolFactory;
            _healthSystem = healthSystem;
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

            _spawnPointTool = _pointToolFactory.Create();
        }
        
        public void OnToolActivation()
        {
            _spawnPointTool.SetEnabled(true);
        }
        
        public void Update()
        {
            foreach (var position in _spawnPointTool.Update())
            {
                SpawnShip(position);
            }
        }

        private void SpawnShip(float2 position)
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
            _spawnPointTool.SetEnabled(false);   
        }

        public void FinalizeTool()
        {
            
        }
    }
}