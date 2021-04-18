using Unity.Collections;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasColorSystem : ISpriteAtlasColorSystem, IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Command;
        public Texture2D Texture { get; private set; }
        public NativeList<SpriteAtlasChunk> Chunks => _indexManager.Chunks;
        
        private readonly SpriteAtlasConfig _config;
        
        private SpriteAtlasIndexManager _indexManager;
        private SpriteAtlasCommandManager _commandManager;

        public SpriteAtlasColorSystem(SpriteAtlasConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
            Texture = new Texture2D(_config.AtlasSize, _config.AtlasSize, _config.AtlasFormat, false, true);
            Texture.name = nameof(SpriteAtlasColorSystem);
            
            var squareManager = new SpriteAtlasSquareManager(_config.AtlasSize);
            _indexManager = new SpriteAtlasIndexManager(squareManager, _config.Chunks);

            _commandManager = new SpriteAtlasCommandManager(this);
        }
        
        public void Update()
        {
            _commandManager.ProcessCommands();
        }
        
        public SpriteAtlasIndex AllocateSpace(int sizeX, int sizeY)
        {
            return _indexManager.AllocateSpace(sizeX, sizeY);
        }

        public void ReleaseSpace(SpriteAtlasIndex spriteAtlasIndex)
        {
            _indexManager.ReleaseSpace(spriteAtlasIndex);
        }

        public void ScheduleTextureCopy(Texture2D source, SpriteAtlasIndex target)
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