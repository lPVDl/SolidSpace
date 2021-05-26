using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    public class ColliderGizmoSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityRender;
        
        private readonly IColliderBakeSystem _bakeSystem;
        private readonly IGizmosManager _gizmosManager;
        private readonly ColliderGizmoSystemConfig _config;

        private GizmosHandle _gizmos;

        public ColliderGizmoSystem(IColliderBakeSystem bakeSystem, IGizmosManager gizmosManager,
            ColliderGizmoSystemConfig config)
        {
            _bakeSystem = bakeSystem;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void InitializeController()
        {
            _gizmos = _gizmosManager.GetHandle(this);
        }

        public void UpdateController()
        {
            if (_config.DrawGrid)
            {
                DrawGrid(_bakeSystem.ColliderWorld.worldGrid);
            }

            if (_config.DrawCollider)
            {
                DrawColliders(_bakeSystem.ColliderWorld);
            }
        }

        private void DrawColliders(ColliderWorld world)
        {
            var color = _config.ColliderColor;
            
            for (var i = 0; i < world.colliderBounds.Length; i++)
            {
                var bounds = world.colliderBounds[i];
                var shape = world.colliderShapes[i];
                var center = new float2(bounds.xMin + bounds.xMax, bounds.yMin + bounds.yMax) / 2f;
                var angle = shape.rotation * FloatMath.TwoPI;
                
                _gizmos.DrawWireRect(center, shape.size, angle, color);
            }
        }
        
        private void DrawGrid(ColliderWorldGrid worldGrid)
        {
            var cellSize = 1 << worldGrid.power;
            var cellCountX = worldGrid.size.x;
            var cellCountY = worldGrid.size.y;
            var worldMin = worldGrid.anchor * cellSize;
            var worldMax = (worldGrid.anchor + worldGrid.size) * cellSize;

            var gridColor = _config.GridColor;
            
            _gizmos.DrawLine(worldMin.x, worldMin.y, worldMin.x, worldMax.y, gridColor);
            _gizmos.DrawLine(worldMin.x, worldMax.y, worldMax.x, worldMax.y, gridColor);
            _gizmos.DrawLine(worldMax.x, worldMax.y, worldMax.x, worldMin.y, gridColor);
            _gizmos.DrawLine(worldMax.x, worldMin.y, worldMin.x, worldMin.y, gridColor);

            for (var i = 1; i < cellCountX; i++)
            {
                var p0 = new float2(worldMin.x + cellSize * i, worldMax.y);
                var p1 = new float2(worldMin.x + cellSize * i, worldMin.y);
                _gizmos.DrawLine(p0, p1, gridColor);
            }
            
            for (var i = 1; i < cellCountY; i++)
            {
                var p2 = new float2(worldMin.x, worldMin.y + i * cellSize);
                var p3 = new float2(worldMax.x, worldMin.y + i * cellSize);
                _gizmos.DrawLine(p2, p3, gridColor);
            }
        }

        public void FinalizeController()
        {
            
        }
    }
}