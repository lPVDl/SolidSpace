using System;
using UnityEngine;

namespace SolidSpace.Entities.Physics.Colliders
{
    [Serializable]
    public class ColliderGizmoSystemConfig
    {
        public bool DrawGrid => _drawGrid;
        public bool DrawCollider => _drawCollider;
        public Color GridColor => _gridColor;
        public Color ColliderColor => _colliderColor;

        [SerializeField] private Color _colliderColor;
        [SerializeField] private Color _gridColor;
        [SerializeField] private bool _drawCollider;
        [SerializeField] private bool _drawGrid;
    }
}