using System;
using Unity.Collections;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasColorSystem : ISpriteAtlasSystem, IDisposable
    {
        public Texture2D Texture { get; private set; }

        public NativeList<SpriteAtlasChunk> Chunks => _indexManager.Chunks;

        private readonly SpriteAtlasIndexManager _indexManager;

        public SpriteAtlasColorSystem()
        {
            var squareManager = new SpriteAtlasSquareManager(9);
            
            _indexManager = new SpriteAtlasIndexManager(squareManager, new[]
            {
                -1, // 1   pixel sprite, 0 items
                -1, // 2   pixel sprite, 0 items
                3,  // 4   pixel sprite, 8x8 items, 32 x 32 pixel chunk
                2,  // 8   pixel sprite, 4x4 items, 32 x 32 pixel chunk
                2,  // 16  pixel sprite, 4x4 items, 64 x 64 pixel chunk
                1,  // 32  pixel sprite, 2x2 items, 64 x 64 pixel chunk
                1,  // 64  pixel sprite, 2x2 items, 128x128 pixel chunk
                0,  // 128 pixel sprite, 1x1 items, 128x128 pixel chunk
            });
            
            Texture = new Texture2D(512, 512, TextureFormat.RGB24, false, true);
            Texture.name = nameof(SpriteAtlasColorSystem);
        }
        
        public SpriteAtlasIndex AllocateSpace(int sizeX, int sizeY)
        {
            return _indexManager.AllocateSpace(sizeX, sizeY);
        }

        public void ReleaseSpace(SpriteAtlasIndex spriteAtlasIndex)
        {
            _indexManager.ReleaseSpace(spriteAtlasIndex);
        }

        public void Dispose()
        {
            _indexManager.Dispose();
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }
    }
}