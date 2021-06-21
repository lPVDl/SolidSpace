using SolidSpace.GameCycle;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    // TODO [T-27]: Move debug related classes to one folder, create facade.
    internal class GizmosManager : IInitializable, IGizmosManager
    {
        private const int BufferSize = 256;
        
        private readonly GizmosConfig _config;
        
        private Material _material;
        private ShapeStorage<Line> _wireLines;
        private ShapeStorage<Rect> _wireRects;
        private ShapeStorage<Polygon> _wirePolygons;
        private ShapeStorage<Square> _wireSquares;

        public GizmosManager(GizmosConfig config)
        {
            _config = config;
        }
        
        public void OnInitialize()
        {
            _wireLines = new ShapeStorage<Line>(BufferSize);
            _wireRects = new ShapeStorage<Rect>(BufferSize);
            _wirePolygons = new ShapeStorage<Polygon>(BufferSize);
            _wireSquares = new ShapeStorage<Square>(BufferSize);
            _material = new Material(_config.Shader);
            
            Camera.onPostRender += OnRender;
        }
        
        private void OnRender(Camera camera)
        {
            _material.SetPass(0);
            
            GizmosWireDrawer.BeginDraw();
            GizmosWireDrawer.DrawLines(_wireLines);
            GizmosWireDrawer.DrawRects(_wireRects);
            GizmosWireDrawer.DrawPolygons(_wirePolygons);
            GizmosWireDrawer.DrawSquares(_wireSquares);
            GizmosWireDrawer.EndDraw();
            
            _wireLines.Clear();
            _wireRects.Clear();
            _wirePolygons.Clear();
            _wireSquares.Clear();
        }

        internal void ScheduleLineDraw(Line line) => _wireLines.Add(line);
        internal void ScheduleWireRectDraw(Rect rect) => _wireRects.Add(rect);
        internal void ScheduleWirePolygonDraw(Polygon polygon) => _wirePolygons.Add(polygon);
        internal void ScheduleWireSquareDraw(Square square) => _wireSquares.Add(square);

        public GizmosHandle GetHandle(object owner)
        {
            return new GizmosHandle(this);
        }

        public void OnFinalize()
        {
            _wireLines.Dispose();
            _wireRects.Dispose();
            _wirePolygons.Dispose();
            _wireSquares.Dispose();
            Object.Destroy(_material);
            
            Camera.onPostRender -= OnRender;
        }
    }
}