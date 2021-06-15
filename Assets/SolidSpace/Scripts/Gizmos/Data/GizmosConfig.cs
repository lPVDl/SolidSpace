using System;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    [Serializable]
    public class GizmosConfig
    {
        public Shader Shader => _shader;

        [SerializeField] private Shader _shader;
    }
}