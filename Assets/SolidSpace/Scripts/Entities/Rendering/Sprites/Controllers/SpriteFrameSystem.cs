using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public class SpriteFrameSystem : ISpriteFrameSystem, IInitializable, IUpdatable
    {
        public Texture2D AtlasTexture => _texture;
        public int2 AtlasSize { get; private set; }
        
        public NativeSlice<AtlasChunk2D> Chunks => _indexManager.Chunks;

        public NativeSlice<ulong> ChunksOccupation => _indexManager.ChunksOccupation;

        private readonly SpriteAtlasConfig _config;
        
        private Texture2D _texture;
        private AtlasIndexManager2D64 _indexManager;
        private bool _isTextureDirty;

        public SpriteFrameSystem(SpriteAtlasConfig config)
        {
            _config = config;
        }

        public void OnInitialize()
        {
            var atlasSize = _config.AtlasConfig.AtlasSize;

            _texture = new Texture2D(atlasSize, atlasSize, TextureFormat.RFloat, false, true);
            _texture.name = nameof(SpriteFrameSystem);
            _texture.filterMode = FilterMode.Point;

            _indexManager = new AtlasIndexManager2D64(_config.AtlasConfig);

            AtlasSize = new int2(atlasSize, atlasSize);
            
            new FillNativeArrayJob<float>
            {
                inValue = 0,
                inTotalItem = atlasSize * atlasSize,
                inItemPerJob = 1024,
                outNativeArray = GetAtlasData(false)
            }.Schedule((int) Math.Ceiling(atlasSize * atlasSize / 1024f), 4).Complete();
        }

        public void OnFinalize()
        {
            // TODO : Create frame disposing via update.
            
            _indexManager.Dispose();
            UnityEngine.Object.Destroy(_texture);
            _texture = null;
        }
        
        public void OnUpdate()
        {
            if (_isTextureDirty)
            {
                _isTextureDirty = false;
                _texture.Apply();
            }
        }

        public NativeArray<float> GetAtlasData(bool readOnly)
        {
            if (!readOnly)
            {
                _isTextureDirty = true;
            }

            return _texture.GetPixelData<float>(0);
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