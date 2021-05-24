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
        public NativeList<AtlasChunk> Chunks => _indexManager.Chunks;
        
        private readonly AtlasConfig _config;
        
        private AtlasIndexManager _indexManager;
        private AtlasCommandManager _commandManager;

        public SpriteColorSystem(AtlasConfig config)
        {
            _config = config;
        }
        
        public void InitializeController()
        {
            Texture = new Texture2D(_config.AtlasSize, _config.AtlasSize, _config.AtlasFormat, false, true);
            Texture.name = nameof(SpriteColorSystem);
            Texture.filterMode = FilterMode.Point;
            
            var squareManager = new AtlasSquareManager(_config.AtlasSize);
            _indexManager = new AtlasIndexManager(squareManager, _config.Chunks);

            _commandManager = new AtlasCommandManager(this);
        }
        
        public void UpdateController()
        {
            _commandManager.ProcessCommands();
        }
        
        public AtlasIndex AllocateSpace(int sizeX, int sizeY)
        {
            return _indexManager.AllocateSpace(sizeX, sizeY);
        }

        public void ReleaseSpace(AtlasIndex atlasIndex)
        {
            _indexManager.ReleaseSpace(atlasIndex);
        }

        public void ScheduleTextureCopy(Texture2D source, AtlasIndex target)
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