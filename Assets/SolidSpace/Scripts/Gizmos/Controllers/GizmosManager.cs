using System.Runtime.CompilerServices;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    // TODO [T-27]: Move debug related classes to one folder, create facade.
    internal class GizmosManager : IInitializable, IGizmosManager
    {
        private const int BufferSize = 256;
        
        private readonly GizmosConfig _config;
        
        private Material _material;
        private NativeArray<GizmosLine> _lines;
        private NativeArray<GizmosRect> _rects;
        private NativeArray<GizmosPolygon> _polygons;
        private int _lineCount;
        private int _rectCount;
        private int _polygonCount;
        
        public GizmosManager(GizmosConfig config)
        {
            _config = config;
        }
        
        public void OnInitialize()
        {
            _lines = NativeMemory.CreatePersistentArray<GizmosLine>(BufferSize);
            _rects = NativeMemory.CreatePersistentArray<GizmosRect>(BufferSize);
            _polygons = NativeMemory.CreatePersistentArray<GizmosPolygon>(BufferSize);
            _material = new Material(_config.Shader);

            Camera.onPostRender += OnRender;
        }
        
        private void OnRender(Camera camera)
        {
            _material.SetPass(0);
            
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);

            DrawLines();
            DrawRects();
            DrawPolygons();

            GL.End();
            GL.PopMatrix();
        }

        private void DrawLines()
        {
            for (var i = 0; i < _lineCount; i++)
            {
                var line = _lines[i];
                GL.Color(line.color);
                GL_Line(line.start, line.end);
            }
            _lineCount = 0;
        }

        private void DrawRects()
        {
            for (var i = 0; i < _rectCount; i++)
            {
                var rect = _rects[i];
                GL.Color(rect.color);
                var center = new float2(rect.center.x, rect.center.y);
                var halfSize = new float2(rect.size.x / 2f, rect.size.y / 2f);
                FloatMath.SinCos(rect.rotationRad, out var sin, out var cos);
                var p0 = center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
                var p1 = center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
                var p2 = center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
                var p3 = center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
                GL_Line(p0, p1);
                GL_Line(p1, p2);
                GL_Line(p2, p3);
                GL_Line(p3, p0);
            }
            _rectCount = 0;
        }

        private void DrawPolygons()
        {
            for (var i = 0; i < _polygonCount; i++)
            {
                var polygon = _polygons[i];
                GL.Color(polygon.color);
                var step = FloatMath.TwoPI / polygon.topology;
                var forward = new float2(polygon.radius, 0);
                var startPoint = polygon.center + forward;
                var prevPoint = startPoint;

                for (var j = 1; j < polygon.topology; j++)
                {
                    FloatMath.SinCos(step * j, out var sin, out var cos);
                    var newPoint = polygon.center + FloatMath.Rotate(forward, sin, cos);
                    GL_Line(prevPoint, newPoint);
                    prevPoint = newPoint;
                }
                
                GL_Line(prevPoint, startPoint);
            }
            
            _polygonCount = 0;
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GL_Line(float2 start, float2 end)
        {
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
        }

        internal void ScheduleLineDraw(GizmosLine line)
        {
            NativeMemory.MaintainPersistentArrayLength(ref _lines, new ArrayMaintenanceData
            {
                itemPerAllocation = BufferSize,
                copyOnResize = true,
                requiredCapacity = _lineCount + 1
            });

            _lines[_lineCount++] = line;
        }

        internal void ScheduleRectDraw(GizmosRect rect)
        {
            NativeMemory.MaintainPersistentArrayLength(ref _rects, new ArrayMaintenanceData
            {
                itemPerAllocation = BufferSize,
                copyOnResize = true,
                requiredCapacity = _rectCount + 1
            });

            _rects[_rectCount++] = rect;
        }

        internal void SchedulePolygonDraw(GizmosPolygon polygon)
        {
            NativeMemory.MaintainPersistentArrayLength(ref _polygons, new ArrayMaintenanceData
            {
                itemPerAllocation = BufferSize,
                copyOnResize = true,
                requiredCapacity = _polygonCount + 1
            });

            _polygons[_polygonCount++] = polygon;
        }

        public GizmosHandle GetHandle(object owner)
        {
            return new GizmosHandle(this);
        }

        public void OnFinalize()
        {
            _lines.Dispose();
            _rects.Dispose();
            _polygons.Dispose();
            Object.Destroy(_material);
            
            Camera.onPostRender -= OnRender;
        }
    }
}