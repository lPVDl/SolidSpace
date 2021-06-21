using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    public struct GizmosHandle
    {
        private readonly GizmosManager _manager;
        
        internal GizmosHandle(GizmosManager manager)
        {
            _manager = manager;
        }

        public void DrawLine(float2 start, float2 end, Color color)
        {
            _manager.ScheduleLineDraw(new GizmosLine
            {
                start = start,
                end = end,
                color = color
            });
        }

        public void DrawLine(float x0, float y0, float x1, float y1, Color color)
        {
            _manager.ScheduleLineDraw(new GizmosLine
            {
                start = new float2(x0, y0),
                end = new float2(x1, y1),
                color = color
            });
        }

        public void DrawWireRect(float2 center, float2 size, float angleRad, Color color)
        {
            _manager.ScheduleRectDraw(new GizmosRect
            {
                center = center,
                size = new half2((half) size.x, (half) size.y),
                color = color,
                rotationRad = (half) angleRad
            });
        }

        public void DrawWireSquare(float2 center, float size, Color color)
        {
            _manager.ScheduleSquareDraw(new GizmosSquare
            {
                center = center,
                size = (half) size,
                color = color
            });
        }

        public void DrawPolygon(float2 center, float radius, int topology, Color color)
        {
            _manager.SchedulePolygonDraw(new GizmosPolygon
            {
                center = center,
                topology = (byte) topology,
                color = color,
                radius = (half) radius
            });
        }
    }
}