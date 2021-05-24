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

        public void DrawLine(float2 start, float2 end, Color32 color)
        {
            _manager.ScheduleDraw(new GizmosShape
            {
                type = EGizmosShapeType.Line,
                float0 = start.x,
                float1 = start.y,
                float2 = end.x,
                float3 = end.y,
                float4 = 0,
                color = color
            });
        }

        public void DrawLine(float x0, float y0, float x1, float y1, Color32 color)
        {
            _manager.ScheduleDraw(new GizmosShape
            {
                type = EGizmosShapeType.Line,
                float0 = x0,
                float1 = y0,
                float2 = x1,
                float3 = y1,
                float4 = 0,
                color = color
            });
        }

        public void DrawWireRect(float2 center, float2 size, float angleRad, Color32 color)
        {
            _manager.ScheduleDraw(new GizmosShape
            {
                type = EGizmosShapeType.Rect,
                float0 = center.x,
                float1 = center.y,
                float2 = size.x,
                float3 = size.y,
                float4 = angleRad,
                color = color
            });
        }
    }
}