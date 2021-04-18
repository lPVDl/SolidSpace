using System;
using UnityEngine;

namespace SpaceSimulator.Entities.Particles.Rendering
{
    [Serializable]
    public class ParticleMeshSystemConfig
    {
        [Serialize] public Shader Shader { get; private set; }
    }
}