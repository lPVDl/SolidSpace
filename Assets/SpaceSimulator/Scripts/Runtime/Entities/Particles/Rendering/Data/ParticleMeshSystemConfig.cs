using System;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [Serializable]
    public class ParticleMeshSystemConfig
    {
        [Serialize] public Shader Shader { get; private set; }
    }
}