using System;
using UnityEngine;

namespace SpaceSimulator.Playground
{
    [Serializable]
    public class ParticleSpawnManagerConfig
    {
        public Vector2 SpawnRangeX => _spawnRangeX;
        public Vector2 SpawnRangeY => _spawnRangeY;
        public float Velocity => _velocity;
        public int SpawnCount => _spawnCount;

        [SerializeField] private float _velocity;
        [SerializeField] private int _spawnCount;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;
    }
}