using System.Collections.Generic;
using System.IO;
using SolidSpace.Entities;
using SolidSpace.Entities.Rendering.Sprites;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Random = UnityEngine.Random;

namespace SolidSpace.Playground
{
    public class SpriteSpawnManager : IInitializable, IUpdatable
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SpriteSpawnManagerConfig _config;
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IEntityManager _entityManager;

        private bool _flushedAtlas;

        public SpriteSpawnManager(SpriteSpawnManagerConfig config, ISpriteColorSystem colorSystem, IEntityManager entityManager)
        {
            _config = config;
            _colorSystem = colorSystem;
            _entityManager = entityManager;
        }
        
        public void Initialize()
        {
            var spriteTexture = _config.SpriteTexture;
            var spriteIndex = _colorSystem.AllocateSpace(spriteTexture.width, spriteTexture.height);
            _colorSystem.ScheduleTextureCopy(spriteTexture, spriteIndex);
            _flushedAtlas = false;

            var typeList = new List<ComponentType>
            {
                typeof(PositionComponent),
                typeof(SpriteRenderComponent),
                typeof(SizeComponent),
            };
            if (_config.RotateSprites)
            {
                typeList.Add(typeof(RotationComponent));
            }
            var archetype = _entityManager.CreateArchetype(typeList.ToArray());

            using var entityArray = _entityManager.CreateEntity(archetype, _config.SpawnCount, Allocator.Temp);

            var sizeX = (half) spriteTexture.width;
            var sizeY = (half) spriteTexture.height;

            foreach (var entity in entityArray)
            {
                var x = Random.Range(_config.SpawnRangeX.x, _config.SpawnRangeX.y);
                var y = Random.Range(_config.SpawnRangeY.x, _config.SpawnRangeY.y);
                
                _entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = new float2(x, y)
                });
                _entityManager.SetComponentData(entity, new SpriteRenderComponent
                {
                    index = spriteIndex,
                });
                _entityManager.SetComponentData(entity, new SizeComponent
                {
                    value = new half2(sizeX, sizeY)
                });

                if (!_config.RotateSprites)
                {
                    continue;
                }
                
                _entityManager.SetComponentData(entity, new RotationComponent
                {
                    value = (half) Random.value
                });
            }
        }
        
        public void Update()
        {
            if (_flushedAtlas)
            {
                return;
            }

            _flushedAtlas = true;
            
            File.WriteAllBytes(_config.OutputAtlasPath, _colorSystem.Texture.EncodeToPNG());
        }
    }
}