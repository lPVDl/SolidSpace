using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Rendering.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    internal class SpriteColorSystem : ISpriteColorSystem, IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        public Texture2D Texture { get; private set; }
        public NativeSlice<AtlasChunk2D> Chunks => _indexManager.Chunks;
        
        private readonly TextureAtlasConfig _config;
        
        private AtlasIndexManager2D _indexManager;
        private TextureAtlasCommandManager _commandManager;

        public SpriteColorSystem(TextureAtlasConfig config)
        {
            _config = config;
        }
        
        public void InitializeController()
        {
            Texture = new Texture2D(_config.AtlasSize, _config.AtlasSize, _config.AtlasFormat, false, true);
            Texture.name = nameof(SpriteColorSystem);
            Texture.filterMode = FilterMode.Point;
            
            var squareManager = new AtlasSectorManager2D(_config.AtlasSize);
            _indexManager = new AtlasIndexManager2D(squareManager, _config.Chunks);

            _commandManager = new TextureAtlasCommandManager(this);
        }
        
        public void UpdateController()
        {
            _commandManager.ProcessCommands();
        }
        
        public AtlasIndex Allocate(int width, int height)
        {
            return _indexManager.Allocate(width, height);
        }

        public void Release(AtlasIndex atlasIndex)
        {
            _indexManager.Release(atlasIndex);
        }

        public void ScheduleCopy(Texture2D source, AtlasIndex target)
        {
            _commandManager.ScheduleTextureCopy(source, target);
        }

        public void FinalizeController()
        {
            _indexManager.Dispose();
            Object.Destroy(Texture);
            Texture = null;
        }
    }
}