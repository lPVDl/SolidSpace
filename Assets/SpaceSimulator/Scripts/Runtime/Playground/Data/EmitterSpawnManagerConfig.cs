using System;
using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    [Serializable]
    public class EmitterSpawnManagerConfig
    {
        public int EntityCount => _entityCount;
        public Material ParticleMaterial => _particleMaterial;
        public float SpawnRate => _spawnRate;
        public Vector2 SpawnRangeX => _spawnRangeX;
        public Vector2 SpawnRangeY => _spawnRangeY;
        public float ParticleVelocity => _particleVelocity;

        [SerializeField] private int _entityCount;
        [SerializeField] private Material _particleMaterial;
        [SerializeField] private float _spawnRate;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;
        [SerializeField] private float _particleVelocity;
    }
}