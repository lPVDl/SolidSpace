using System;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [Serializable]
    public class SpriteMeshSystemConfig
    {
        public Shader Shader => _shader;
        
        [SerializeField] private Shader _shader;
    }
}