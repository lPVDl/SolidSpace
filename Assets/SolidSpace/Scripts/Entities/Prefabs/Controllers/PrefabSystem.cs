using System;
using System.Collections.Generic;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Prefabs
{
    internal class PrefabSystem : IPrefabSystem, IInitializable, IUpdatable
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
        private NativeArray<byte> _shipHealth;
        private int2 _shipSize;
        private List<PrefabReplicationData> _replications;

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

            var byteCount = HealthUtil.GetRequiredByteCount(texture.width, texture.height);
            _shipHealth = NativeMemory.CreatePersistentArray<byte>(byteCount);

            if (texture.format != TextureFormat.RGB24)
            {
                var message = $"Texture expected to have format RGB24, but was {texture.format}.";
                throw new InvalidOperationException(message);
            }

            var pixels = texture.GetPixelData<ColorRGB24>(0);
            HealthUtil.TextureToHealth(pixels, texture.width, texture.height, _shipHealth);

            ShipComponents = shipComponents;

            _replications = new List<PrefabReplicationData>();
        }

        public void OnFinalize()
        {
            _colorSystem.Release(_shipColorIndex);
            _shipHealth.Dispose();
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

            new BlitHealthToFrameJob
            {
                inHealth = _shipHealth,
                inHealthSize = _shipSize,
                inAtlasSize = _frameSystem.AtlasSize,
                inOutAtlasTexture = _frameSystem.GetAtlasData(false),
                inAtlasOffset = AtlasMath.ComputeOffset(_frameSystem.Chunks, frameIndex),
            }.Run();

            var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, healthIndex);
            var healthAtlas = _healthSystem.Data;
            for (var i = 0; i < _shipHealth.Length; i++)
            {
                healthAtlas[healthOffset + i] = _shipHealth[i];
            }
        }
        
        public void ScheduleReplication(PrefabReplicationData replicationData)
        {
            _replications.Add(replicationData);
        }

        public void OnUpdate()
        {
            foreach (var replication in _replications)
            {
                var parentEntity = replication.parent;
                var parentPrefab = _entityManager.GetComponentData<PrefabInstanceComponent>(parentEntity);
                var parentPosition = _entityManager.GetComponentData<PositionComponent>(parentEntity).value;
                var parentRotation = _entityManager.GetComponentData<RotationComponent>(parentEntity).value;
                var parentSize = _entityManager.GetComponentData<RectSizeComponent>(parentEntity).value;
                
                var childEntity = _entityManager.CreateEntity(_shipArchetype);
                var childBounds = replication.childBounds;
                var childHealthIndex = replication.childHealth;
                var childSize = childBounds.GetSize();
                
                _entityManager.SetComponentData(childEntity, new RectSizeComponent
                {
                    value = new half2((half) childSize.x, (half) childSize.y)
                });
                
                var childLocalPosX = (childBounds.max.x + childBounds.min.x + 1) * 0.5f - parentSize.x * 0.5f;
                var childLocalPosY = (childBounds.max.y + childBounds.min.y + 1) * 0.5f - parentSize.y * 0.5f;
                FloatMath.SinCos(parentRotation, out var childSin, out var childCos);
                _entityManager.SetComponentData(childEntity, new PositionComponent
                {
                    value = FloatMath.Rotate(childLocalPosX, childLocalPosY, childSin, childCos) + parentPosition
                });
                
                _entityManager.SetComponentData(childEntity, new RotationComponent
                {
                    value = parentRotation
                });
                
                _entityManager.SetComponentData(childEntity, new HealthComponent
                {
                    index = childHealthIndex
                });

                var childFrameIndex = _frameSystem.Allocate(childSize.x, childSize.y);
                var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, childHealthIndex);
                var healthSize = HealthUtil.GetRequiredByteCount(childSize.x, childSize.y);
                new BlitHealthToFrameJob
                {
                    inHealth = _healthSystem.Data.Slice(healthOffset, healthSize),
                    inOutAtlasTexture = _frameSystem.GetAtlasData(false),
                    inAtlasOffset = AtlasMath.ComputeOffset(_frameSystem.Chunks, childFrameIndex),
                    inAtlasSize = _frameSystem.AtlasSize,
                    inHealthSize = childSize,
                }.Run();
                _entityManager.SetComponentData(childEntity, new SpriteRenderComponent
                {
                    colorIndex = _shipColorIndex,
                    frameIndex = childFrameIndex
                });
                
                _entityManager.SetComponentData(childEntity, new PrefabInstanceComponent
                {
                    prefabIndex = 0,
                    instanceOffset = parentPrefab.instanceOffset + childBounds.min
                });
            }

            _replications.Clear();
        }
    }
}