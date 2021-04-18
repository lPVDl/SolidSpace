using System;
using UnityEngine;

namespace SpaceSimulator.Entities.Particles.Rendering
{
    [Serializable]
    public class ParticleMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}