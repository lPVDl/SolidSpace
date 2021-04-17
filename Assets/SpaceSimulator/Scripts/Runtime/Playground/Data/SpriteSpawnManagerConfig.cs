using System;
using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    [Serializable]
    public class SpriteSpawnManagerConfig
    {
        [Serialize] public Texture2D SpriteTexture { get; private set; }
        [Serialize] public string OutputAtlasPath { get; private set; }
    }
}