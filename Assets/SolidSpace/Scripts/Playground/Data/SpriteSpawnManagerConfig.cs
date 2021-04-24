using System;
using UnityEngine;

namespace SolidSpace.Playground
{
    [Serializable]
    public class SpriteSpawnManagerConfig
    {
        public Texture2D SpriteTexture => _spriteTexture;
        public string OutputAtlasPath => _outputAtlasPath;
        public int SpawnCount => _spawnCount;
        public Vector2 SpawnRangeX => _spawnRangeX;
        public Vector2 SpawnRangeY => _spawnRangeY;
        public bool RotateSprites => _rotateSprites;
        
        [SerializeField] private Texture2D _spriteTexture;
        [SerializeField] private string _outputAtlasPath;
        [SerializeField] private int _spawnCount;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;
        [SerializeField] private bool _rotateSprites;
    }
}