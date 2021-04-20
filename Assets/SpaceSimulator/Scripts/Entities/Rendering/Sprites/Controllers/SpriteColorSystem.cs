using SpaceSimulator.Entities.EntityWorld;
using SpaceSimulator.Entities.Rendering.Atlases;
using Unity.Collections;
using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Sprites
{
    public class SpriteColorSystem : ISpriteColorSystem, IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;
        public Texture2D Texture { get; private set; }
        public NativeList<AtlasChunk> Chunks => _indexManager.Chunks;
        
        private readonly AtlasConfig _config;
        
        private AtlasIndexManager _indexManager;
        private AtlasCommandManager _commandManager;

        public SpriteColorSystem(AtlasConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
            Texture = new Texture2D(_config.AtlasSize, _config.AtlasSize, _config.AtlasFormat, false, true);
            Texture.name = nameof(SpriteColorSystem);
            
            var squareManager = new AtlasSquareManager(_config.AtlasSize);
            _indexManager = new AtlasIndexManager(squareManager, _config.Chunks);

            _commandManager = new AtlasCommandManager(this);
        }
        
        public void Update()
        {
            _commandManager.ProcessCommands();
        }
        
        public AtlasIndex AllocateSpace(int sizeX, int sizeY)
        {
            return _indexManager.AllocateSpace(sizeX, sizeY);
        }

        public void ReleaseSpace(AtlasIndex spriteAtlasIndex)
        {
            _indexManager.ReleaseSpace(spriteAtlasIndex);
        }

        public void ScheduleTextureCopy(Texture2D source, AtlasIndex target)
        {
            _commandManager.ScheduleTextureCopy(source, target);
        }

        public void FinalizeSystem()
        {
            _indexManager.Dispose();
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }
    }
}