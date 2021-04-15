using System;
using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    [Serializable]
    public class SpriteSpawnManagerConfig
    {
        public Texture2D SpriteTexture => _spriteTexture;
        public string OutputAtlasPath => _outputAtlasPath;
        
        [SerializeField] private Texture2D _spriteTexture;
        [SerializeField] private string _outputAtlasPath;
    }
}