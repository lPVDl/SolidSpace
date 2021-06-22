using SolidSpace.Gizmos.Shapes;
using Unity.Mathematics;
using UnityEngine;
using Rect = SolidSpace.Gizmos.Shapes.Rect;

namespace SolidSpace.Gizmos
{
    public struct GizmosHandle
    {
        private readonly GizmosManager _manager;
        private readonly Color _color;
        
        internal GizmosHandle(GizmosManager manager, Color color)
        {
            _manager = manager;
            _color = color;
        }

        public void DrawLine(float2 start, float2 end)
        {
            _manager.ScheduleLineDraw(new Line
            {
                start = start,
                end = end,
                color = _color
            });
        }

        public void DrawLine(float x0, float y0, float x1, float y1)
        {
            _manager.ScheduleLineDraw(new Line
            {
                start = new float2(x0, y0),
                end = new float2(x1, y1),
                color = _color
            });
        }

        public void DrawWireRect(float2 center, float2 size, float angleRad)
        {
            _manager.ScheduleWireRectDraw(new Rect
            {
                center = center,
                size = new half2((half) size.x, (half) size.y),
                color = _color,
                rotationRad = (half) angleRad
            });
        }

        public void DrawWireSquare(float2 center, float size)
        {
            _manager.ScheduleWireSquareDraw(new Square
            {
                center = center,
                size = (half) size,
                color = _color
            });
        }

        public void DrawScreenSquare(float2 center, float size)
        {
            _manager.ScheduleScreenSquareDraw(new Square
            {
                center = center,
                size = (half) size,
                color = _color
            });
        }

        public void DrawWirePolygon(float2 center, float radius, int topology)
        {
            _manager.ScheduleWirePolygonDraw(new Polygon
            {
                center = center,
                topology = (byte) topology,
                color = _color,
                radius = (half) radius
            });
        }
    }
}