using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    // TODO [T-27]: Move debug related classes to one folder, create facade.
    internal class GizmosManager : IInitializable, IUpdatable, IGizmosManager
    {
        private List<GizmosShape>[] _records;
        private EGizmosShapeType[] _shapeTypes;
        
        public void Initialize()
        {
            _shapeTypes = (EGizmosShapeType[]) Enum.GetValues(typeof(EGizmosShapeType));
            _records = new List<GizmosShape>[_shapeTypes.Length + 1];
            for (var i = 1; i < _records.Length; i++)
            {
                _records[i] = new List<GizmosShape>();
            }
        }

        public void Update()
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
                var center = new float2(square.float0, square.float1);
                var halfSize = new float2(square.float2 / 2f, square.float3 / 2f);
                FloatMath.SinCos(square.float4, out var sin, out var cos);
                var p0 = center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
                var p1 = center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
                var p2 = center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
                var p3 = center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
                DrawLine(p0, p1, square.color);
                DrawLine(p1, p2, square.color);
                DrawLine(p2, p3, square.color);
                DrawLine(p3, p0, square.color);
            }
            squares.Clear();
        }

        private void DrawLine(float2 start, float2 end, Color color)
        {
            var p0 = new Vector3(start.x, start.y, 0);
            var p1 = new Vector3(end.x, end.y, 0);
            Debug.DrawLine(p0, p1, color);
        }

        internal void ScheduleDraw(GizmosShape shape)
        {
            _records[(int)shape.type].Add(shape);
        }

        public GizmosHandle GetHandle(object owner)
        {
            return new GizmosHandle(this);
        }

        public void Finalize() { }
    }
}