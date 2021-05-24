using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    // TODO [T-27]: Move debug related classes to one folder, create facade.
    internal class GizmosManager : IGizmosManager, IDisposable
    {
        private readonly GizmosBehaviour _behaviour;
        private readonly List<GizmosShape>[] _records;
        private readonly EGizmosShapeType[] _shapeTypes;
        
        public GizmosManager()
        {
            _shapeTypes = Enum.GetValues(typeof(EGizmosShapeType)) as EGizmosShapeType[];
            _records = new List<GizmosShape>[_shapeTypes.Length + 1];
            for (var i = 1; i < _records.Length; i++)
            {
                _records[i] = new List<GizmosShape>();
            }

            var gameObject = new GameObject(nameof(GizmosManager));
            _behaviour = gameObject.AddComponent<GizmosBehaviour>();
            _behaviour.DrawGizmos += OnDrawGizmos;
        }

        private void OnDrawGizmos()
        {
            var lines = _records[(int) EGizmosShapeType.Line];
            foreach (var line in lines)
            {
                var start = new Vector3(line.float0, line.float1, 0);
                var end = new Vector3(line.float2, line.float3, 0);
                Debug.DrawLine(start, end, line.color, 0);
            }
            lines.Clear();
            
            var squares = _records[(int) EGizmosShapeType.Rect];
            foreach (var square in squares)
            {
                var center = new Vector3(square.float0, square.float1, 0);
                var size = new Vector3(square.float2, square.float3, 1);
                UnityEngine.Gizmos.color = square.color;
                UnityEngine.Gizmos.DrawWireCube(center, size);
            }
            squares.Clear();
        }

        internal void SheduleDraw(GizmosShape shape)
        {
            _records[(int)shape.type].Add(shape);
        }

        public GizmosHandle GetHandle(object owner)
        {
            return new GizmosHandle(this);
        }

        public void Dispose()
        {
            _behaviour.DrawGizmos -= OnDrawGizmos;
        }
    }
}