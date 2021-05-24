using System;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Colliders
{
    [Serializable]
    public class ColliderBakeSystemConfig
    {
        public Color GridColor => _gridColor;
        
        [SerializeField] private Color _gridColor;
    }
}