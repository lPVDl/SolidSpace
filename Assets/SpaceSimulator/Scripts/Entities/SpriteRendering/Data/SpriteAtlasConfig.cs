using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceSimulator.Entities.SpriteRendering
{
    [Serializable]
    public class SpriteAtlasConfig
    {
        [Serialize] public int AtlasSize { get; private set; }
        [Serialize] public TextureFormat AtlasFormat { get; private set; }
        [Serialize] public IReadOnlyList<SpriteAtlasChunkConfig> Chunks { get; private set; }
    }
}