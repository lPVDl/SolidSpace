using System;
using System.Collections.Generic;
using SolidSpace.Entities.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Atlases
{
    [Serializable]
    public class TextureAtlasConfig
    {
        public int AtlasSize => _atlasSize;
        public TextureFormat AtlasFormat => _atlasFormat;
        public IReadOnlyList<AtlasChunk2DConfig> Chunks => _chunks;
        
        [SerializeField] private int _atlasSize;
        [SerializeField] private TextureFormat _atlasFormat;
        [SerializeField] private List<AtlasChunk2DConfig> _chunks;
    }
}