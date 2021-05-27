using System;
using System.Collections.Generic;
using SolidSpace.Entities.Atlases;
using UnityEngine;

namespace SolidSpace.Entities.Health.Atlases
{
    [Serializable]
    public class LinearAtlasConfig
    {
        public int AtlasSize => _atlasSize;
        public IReadOnlyList<AtlasChunk1DConfig> Chunks => _chunks; 
        
        [SerializeField] private int _atlasSize;
        [SerializeField] private List<AtlasChunk1DConfig> _chunks;
    }
}