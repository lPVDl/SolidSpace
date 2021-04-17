using System;
using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    [Serializable]
    public class EmitterSpawnManagerConfig
    {
        [Serialize] public int EntityCount { get; private set; }
        [Serialize] public Material ParticleMaterial { get; private set; }
        [Serialize] public float SpawnRate { get; private set; }
        [Serialize] public Vector2 SpawnRangeX { get; private set; }
        [Serialize] public Vector2 SpawnRangeY { get; private set; }
        [Serialize] public float ParticleVelocity { get; private set; }
    }
}