using System.Collections.Generic;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Prefabs
{
    internal class PrefabSystem : IPrefabSystem, IInitializable
    {
        public IReadOnlyList<ComponentType> ShipComponents { get; private set; }

        public int2 ShipSize => _shipSize;

        private readonly PrefabSystemConfig _config;
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IEntityManager _entityManager;
        private readonly ISpriteFrameSystem _frameSystem;
        private readonly IHealthAtlasSystem _healthSystem;

        private EntityArchetype _shipArchetype;
        private AtlasIndex16 _shipColorIndex;
        private int2 _shipSize;

        public PrefabSystem(PrefabSystemConfig config, 
                            ISpriteColorSystem colorSystem,
                            IEntityManager entityManager,
                            ISpriteFrameSystem frameSystem,
                            IHealthAtlasSystem healthSystem)
        {
            _config = config;
            _colorSystem = colorSystem;
            _entityManager = entityManager;
            _frameSystem = frameSystem;
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
                typeof(RigidbodyComponent),
                typeof(PrefabInstanceComponent)
            };
            
            _shipArchetype = _entityManager.CreateArchetype(shipComponents);
            var texture = _config.ShipTexture;
            _shipSize = new int2(texture.width, texture.height);
            _shipColorIndex = _colorSystem.Allocate(_shipSize.x, _shipSize.y);
            _colorSystem.Copy(texture, _shipColorIndex);
            // TODO: Prebake frame here...
            // TODO: Prebake health here...
            
            ShipComponents = shipComponents;
        }

        public void OnFinalize()
        {
            _colorSystem.Release(_shipColorIndex);
        }
        
        public void SpawnShip(float2 position, float rotation)
        {
            var frameIndex = _frameSystem.Allocate(_shipSize.x, _shipSize.y);
            var healthIndex = _healthSystem.Allocate(_shipSize.x, _shipSize.y);
            var entity = _entityManager.CreateEntity(_shipArchetype);
            
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RectSizeComponent
            {
                value = new half2((half)_shipSize.x, (half)_shipSize.y)
            });
            _entityManager.SetComponentData(entity, new RotationComponent
            {
                value = rotation
            });
            _entityManager.SetComponentData(entity, new SpriteRenderComponent
            {
                colorIndex = _shipColorIndex,
                frameIndex = frameIndex
            });
            _entityManager.SetComponentData(entity, new PrefabInstanceComponent
            {
                prefabIndex = 0,
                instanceOffset = byte2.zero
            });
            _entityManager.SetComponentData(entity, new HealthComponent
            {
                index = healthIndex
            });
            _entityManager.SetComponentData(entity, new ActorComponent
            {
                isActive = false
            });
            
            // TODO: Initialize frame...
            // TODO: Initialize health...
        }
        
        public void ScheduleReplication(EntityReplicationData replicationData)
        {
            
        }
    }
}