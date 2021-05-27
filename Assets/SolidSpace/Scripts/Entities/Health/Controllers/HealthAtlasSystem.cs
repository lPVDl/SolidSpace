using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Health.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    internal class HealthAtlasSystem : IHealthAtlasSystem, IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        public NativeArray<byte> Data => _data;
        public NativeSlice<AtlasChunk1D> Chunks => _indexManager.Chunks;
        
        private readonly LinearAtlasConfig _config;
        
        private AtlasIndexManager1D _indexManager;
        private NativeArray<byte> _data;

        public HealthAtlasSystem(LinearAtlasConfig config)
        {
            _config = config;
        }
        
        public void InitializeController()
        {
            _data = NativeMemory.CreatePersistentArray<byte>(_config.AtlasSize);
            _indexManager = new AtlasIndexManager1D(_config.AtlasSize, _config.Chunks);
        }

        public void UpdateController()
        {
            
        }
        
        public void Copy(Texture2D source, AtlasIndex target)
        {
            if (source.format != TextureFormat.RGB24)
            {
                var message = $"Expected texture with format {TextureFormat.RGB24}, but got {source.format}";
                throw new InvalidOperationException(message);
            }
            
            var chunk = _indexManager.Chunks[target.chunkId];
            chunk.GetPower(out var indexPower, out var itemPower);
            var itemMaxSize = 1 << itemPower;
            var textureSize = source.width * source.height;
            if (textureSize > itemMaxSize)
            {
                var message = $"Expected texture with size less than {itemMaxSize}, but got {textureSize}";
                throw new InvalidOperationException(message);
            }

            var offset = AtlasMath.ComputeOffset(chunk, target);
            var textureRaw = source.GetRawTextureData<ColorRGB24>();
            for (var i = 0; i < textureSize; i++)
            {
                var color = textureRaw[i];
                _data[offset + i] = (byte) Math.Min(1, color.r + color.g + color.b);
            }
        }
        
        public AtlasIndex Allocate(int size)
        {
            return _indexManager.Allocate(size);
        }

        public void Release(AtlasIndex index)
        {
            _indexManager.Release(index);
        }

        public void FinalizeController()
        {
            Data.Dispose();
        }
    }
}