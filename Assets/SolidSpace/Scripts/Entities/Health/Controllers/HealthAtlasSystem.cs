using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Splitting;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    internal class HealthAtlasSystem : IHealthAtlasSystem, IInitializable
    {
        public NativeArray<byte> Data => _data;
        public NativeSlice<AtlasChunk1D> Chunks => _indexManager.Chunks;
        public NativeSlice<ushort> ChunksOccupation => _indexManager.ChunksOccupation;
        
        private readonly Atlas1DConfig _config;
        
        private AtlasIndexManager1D _indexManager;
        private NativeArray<byte> _data;

        public HealthAtlasSystem(Atlas1DConfig config)
        {
            _config = config;
        }
        
        public void OnInitialize()
        {
            _data = NativeMemory.CreatePersistentArray<byte>(_config.AtlasSize);
            _indexManager = new AtlasIndexManager1D(_config);
        }

        public void Copy(Texture2D source, AtlasIndex target)
        {
            if (source.format != TextureFormat.RGB24)
            {
                var message = $"Expected texture with format {TextureFormat.RGB24}, but got {source.format}";
                throw new InvalidOperationException(message);
            }
            
            var chunk = _indexManager.Chunks[target.chunkId];
            var itemMaxSize = 1 << chunk.itemPower;
            var textureSize = new int2(source.width, source.height);
            var requiredByteCount = HealthFrameBitsUtil.GetRequiredByteCount(textureSize.x, textureSize.y);
            
            if (requiredByteCount > itemMaxSize)
            {
                var message = $"Given texture {textureSize} requires {requiredByteCount} bytes, but {itemMaxSize} is available";
                throw new InvalidOperationException(message);
            }

            var offset = AtlasMath.ComputeOffset(chunk, target);
            var textureRaw = source.GetRawTextureData<ColorRGB24>();
            var atlasSlice = new NativeSlice<byte>(_data, offset, itemMaxSize);
            HealthFrameBitsUtil.TextureToFrameBits(textureRaw, textureSize.x, textureSize.y, atlasSlice);
        }

        public AtlasIndex Allocate(int width, int height)
        {
            var size = HealthFrameBitsUtil.GetRequiredByteCount(width, height);
            return _indexManager.Allocate(size);
        }

        public void Release(AtlasIndex index)
        {
            _indexManager.Release(index);
        }

        public void OnFinalize()
        {
            _indexManager.Dispose();
            Data.Dispose();
        }
    }
}