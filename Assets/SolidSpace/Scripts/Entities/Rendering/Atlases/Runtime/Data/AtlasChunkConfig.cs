using System;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Atlases
{
    [Serializable]
    public struct AtlasChunkConfig
    {
        [SerializeField] public int spriteSize;
        [SerializeField] public int itemCount;
    }
}