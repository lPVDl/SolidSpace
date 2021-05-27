using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Rendering.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SolidSpace.Entities.Rendering.Sprites
{
    internal class SpriteColorSystem : ISpriteColorSystem, IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        public Texture2D Texture { get; private set; }
        public NativeSlice<AtlasChunk2D> Chunks => _indexManager.Chunks;
        
        private readonly TextureAtlasConfig _config;
        
        private AtlasIndexManager2D _indexManager;

        public SpriteColorSystem(TextureAtlasConfig config)
        {
            _config = config;
        }
        
        public void InitializeController()
        {
            Texture = new Texture2D(_config.AtlasSize, _config.AtlasSize, _config.AtlasFormat, false, true);
            Texture.name = nameof(SpriteColorSystem);
            Texture.filterMode = FilterMode.Point;
            
            _indexManager = new AtlasIndexManager2D(_config.AtlasSize, _config.Chunks);
        }
        
        public void UpdateController()
        {
            
        }
        
        public AtlasIndex Allocate(int width, int height)
        {
            return _indexManager.Allocate(width, height);
        }

        public void Release(AtlasIndex index)
        {
            _indexManager.Release(index);
        }

        public void Copy(Texture2D source, AtlasIndex target)
        {
            var atlasFormat = _config.AtlasFormat;
            
            if (source.format != atlasFormat)
            {
                var message = $"Expected texture with format {atlasFormat} but got {source.format}";
                throw new InvalidOperationException(message);
            }

            var chunk = _indexManager.Chunks[target.chunkId];
            chunk.GetPower(out var indexPower, out var itemPower);
            var itemMaxSize = 1 << itemPower;
            if (source.width > itemMaxSize || source.height > itemMaxSize)
            {
                var message = $"Expected texture with size less that {itemMaxSize}x{itemMaxSize}, but got {source.width}x{source.height}";
                throw new InvalidOperationException(message);
            }
            
            var offset = AtlasMath.ComputeOffset(chunk, target);
            Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, 
                Texture, 0, 0, offset.x, offset.y);
        }

        public void FinalizeController()
        {
            _indexManager.Dispose();
            Object.Destroy(Texture);
            Texture = null;
        }
    }
}