using System;
using UnityEngine;

namespace SolidSpace.Entities.Atlases
{
    [Serializable]
    public struct AtlasChunk1DConfig
    {
        [SerializeField] public int itemSize;
        [SerializeField] public int itemCount;
    }
}