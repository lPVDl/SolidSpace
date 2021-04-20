using System;
using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Particles
{
    [Serializable]
    public class ParticleMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}