using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Atlases
{
    [Serializable]
    public class AtlasConfig
    {
        public int AtlasSize => _atlasSize;
        public TextureFormat AtlasFormat => _atlasFormat;
        public IReadOnlyList<AtlasChunkConfig> Chunks => _chunks;
        
        [SerializeField] private int _atlasSize;
        [SerializeField] private TextureFormat _atlasFormat;
        [SerializeField] private List<AtlasChunkConfig> _chunks;
    }
}