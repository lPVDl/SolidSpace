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

        public void DrawLine(Vector2 from, Vector2 to, Color32 color)
        {
            _manager.SheduleDraw(new GizmosShape
            {
                type = EGizmosShapeType.Line,
                float0 = from.x,
                float1 = from.y,
                float2 = to.x,
                float3 = to.y,
                color = color
            });
        }

        public void DrawLine(float x0, float y0, float x1, float y1, Color32 color)
        {
            _manager.SheduleDraw(new GizmosShape
            {
                type = EGizmosShapeType.Line,
                float0 = x0,
                float1 = y0,
                float2 = x1,
                float3 = y1,
                color = color
            });
        }

        public void DrawWireRect(Vector2 center, Vector2 size, Color color)
        {
            _manager.SheduleDraw(new GizmosShape
            {
                type = EGizmosShapeType.Rect,
                float0 = center.x,
                float1 = center.y,
                float2 = size.x,
                float3 = size.y,
                color = color
            });
        }
    }
}