using System.Runtime.CompilerServices;
using SolidSpace.Gizmos.Shapes;
using Unity.Mathematics;
using UnityEngine;

using Rect = SolidSpace.Gizmos.Shapes.Rect;

namespace SolidSpace.Gizmos
{
    public struct GizmosHandle
    {
        internal readonly ushort id;
        
        private readonly GizmosManager _handleFactory;
        private readonly IGizmosStateStorage _storage;

        private int _cashVersion;
        private bool _enabled;
        private Color _color;
        
        internal GizmosHandle(ushort id, GizmosManager handleFactory, IGizmosStateStorage storage)
        {
            this.id = id;
            _handleFactory = handleFactory;
            _storage = storage;
            _cashVersion = -1;
            _enabled = false;
            _color = Color.clear;
        }

        public void DrawLine(float2 start, float2 end)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _handleFactory.ScheduleLineDraw(new Line
            {
                start = start,
                end = end,
                color = _color
            });
        }

        public void DrawLine(float x0, float y0, float x1, float y1)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _handleFactory.ScheduleLineDraw(new Line
            {
                start = new float2(x0, y0),
                end = new float2(x1, y1),
                color = _color
            });
        }

        public void DrawWireRect(float2 center, float2 size, float angleRad)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _handleFactory.ScheduleWireRectDraw(new Rect
            {
                center = center,
                size = new half2((half) size.x, (half) size.y),
                color = _color,
                rotationRad = (half) angleRad
            });
        }

        public void DrawWireSquare(float2 center, float size)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _handleFactory.ScheduleWireSquareDraw(new Square
            {
                center = center,
                size = (half) size,
                color = _color
            });
        }

        public void DrawScreenSquare(float2 center, float size)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _handleFactory.ScheduleScreenSquareDraw(new Square
            {
                center = center,
                size = (half) size,
                color = _color
            });
        }

        public void DrawWirePolygon(float2 center, float radius, int topology)
        {
            UpdateCash();
            if (!_enabled)
            {
                return;
            }
            
            _handleFactory.ScheduleWirePolygonDraw(new Polygon
            {
                center = center,
                topology = (byte) topology,
                color = _color,
                radius = (half) radius
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCash()
        {
            var managerVersion = _handleFactory.RenderVersion;
            if (managerVersion == _cashVersion)
            {
                return;
            }

            _cashVersion = managerVersion;
            _enabled = _storage.GetHandleEnabled(id);
            _color = _storage.GetHandleColor(id);
        }
    }
}