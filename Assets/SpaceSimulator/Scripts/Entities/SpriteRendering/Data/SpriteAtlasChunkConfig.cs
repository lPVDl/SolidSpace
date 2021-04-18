using System;

namespace SpaceSimulator.Entities.SpriteRendering
{
    [Serializable]
    public struct SpriteAtlasChunkConfig
    {
        [Serialize] public int spriteSize;
        [Serialize] public int itemCount;
    }
}