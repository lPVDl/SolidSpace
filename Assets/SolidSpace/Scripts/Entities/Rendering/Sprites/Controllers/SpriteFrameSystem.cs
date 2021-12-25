using SolidSpace.Entities.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public class SpriteFrameSystem : ISpriteFrameSystem, IInitializable
    {
        public Texture2D Texture { get; private set; }

        public NativeSlice<AtlasChunk2D> Chunks => _indexManager.Chunks;

        public NativeSlice<ulong> ChunksOccupation => _indexManager.ChunksOccupation;

        private readonly SpriteAtlasConfig _config;
        
        private AtlasIndexManager2D64 _indexManager;

        public SpriteFrameSystem(SpriteAtlasConfig config)
        {
            _config = config;
        }

        public void OnInitialize()
        {
            var atlasSize = _config.AtlasConfig.AtlasSize;

            Texture = new Texture2D(atlasSize, atlasSize, _config.TextureFormat, false, true);
            Texture.name = nameof(SpriteFrameSystem);
            Texture.filterMode = FilterMode.Point;

            _indexManager = new AtlasIndexManager2D64(_config.AtlasConfig);
        }

        public void OnFinalize()
        {
            // TODO : Create frame disposing via update.
            
            _indexManager.Dispose();
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }

        public AtlasIndex64 Allocate(int width, int height)
        {
            return _indexManager.Allocate(width, height);
        }

        public void Release(AtlasIndex64 index64)
        {
            _indexManager.Release(index64);
        }
    }
}