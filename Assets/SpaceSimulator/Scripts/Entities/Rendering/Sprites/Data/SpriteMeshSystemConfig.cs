using System;
using UnityEngine;

namespace SpaceSimulator.Entities.Rendering.Sprites
{
    [Serializable]
    public class SpriteMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}