using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimulator.Entities.SpriteRendering
{
    [Serializable]
    public class SpriteAtlasConfig
    {
        public int AtlasSize => _atlasSize;
        public TextureFormat AtlasFormat => _atlasFormat;
        public IReadOnlyList<SpriteAtlasChunkConfig> Chunks => _chunks;
        
        [SerializeField] private int _atlasSize;
        [SerializeField] private TextureFormat _atlasFormat;
        [SerializeField] private List<SpriteAtlasChunkConfig> _chunks;
    }
}