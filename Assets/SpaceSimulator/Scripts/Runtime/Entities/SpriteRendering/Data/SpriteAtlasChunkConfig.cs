using System;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    [Serializable]
    public struct SpriteAtlasChunkConfig
    {
        [Serialize] public int spriteSize;
        [Serialize] public int itemCount;
    }
}