using System.IO;
using SpaceSimulator.Entities.Rendering.Sprites;
using UnityEngine;

namespace SpaceSimulator.Playground
{
    public class SpriteSpawnManager : IInitializable, IUpdatable
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SpriteSpawnManagerConfig _config;
        private readonly ISpriteColorSystem _colorSystem;

        private bool _flushedAtlas;

        public SpriteSpawnManager(SpriteSpawnManagerConfig config, ISpriteColorSystem colorSystem)
        {
            _config = config;
            _colorSystem = colorSystem;
        }
        
        public void Initialize()
        {
            var spriteTexture = _config.SpriteTexture;
            var spriteIndex = _colorSystem.AllocateSpace(spriteTexture.width, spriteTexture.height);
            _colorSystem.ScheduleTextureCopy(spriteTexture, spriteIndex);
            _flushedAtlas = false;
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