using System;
using UnityEngine;

namespace SolidSpace.Playground
{
    [Serializable]
    public class ShipSpawnManagerConfig
    {
        public bool MouseControl => _mouseControl;
        public Texture2D ShipTexture => _shipTexture;
        public Vector2 SpawnRangeX => _spawnRangeX;
        public Vector2 SpawnRangeY => _spawnRangeY;
        public int SpawnCount => _spawnCount;

        [SerializeField] private int _spawnCount;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;
        [SerializeField] private bool _mouseControl;
        [SerializeField] private Texture2D _shipTexture;
    }
}