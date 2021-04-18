using System;
using UnityEngine;

namespace SpaceSimulator.Entities.SpriteRendering
{
    [Serializable]
    public struct SpriteAtlasChunkConfig
    {
        [SerializeField] public int spriteSize;
        [SerializeField] public int itemCount;
    }
}