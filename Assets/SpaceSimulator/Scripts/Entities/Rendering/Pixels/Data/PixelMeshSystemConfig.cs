using System;
using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Pixels
{
    [Serializable]
    public class PixelMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}