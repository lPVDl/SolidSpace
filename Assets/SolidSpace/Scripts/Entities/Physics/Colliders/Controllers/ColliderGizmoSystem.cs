using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    public class ColliderGizmoSystem : IInitializable, IUpdatable
    {
        private readonly IColliderSystem _bakeSystem;
        private readonly IGizmosManager _gizmosManager;
        private readonly ColliderGizmoSystemConfig _config;

        private GizmosHandle _gridGizmos;
        private GizmosHandle _colliderGizmos;

        public ColliderGizmoSystem(IColliderSystem bakeSystem, IGizmosManager gizmosManager,
            ColliderGizmoSystemConfig config)
        {
            _bakeSystem = bakeSystem;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void OnInitialize()
        {
            _gridGizmos = _gizmosManager.GetHandle(this, "Grid", _config.GridColor);
            _colliderGizmos = _gizmosManager.GetHandle(this, "Colliders", _config.ColliderColor);
        }

        public void OnUpdate()
        {
            if (_config.DrawGrid)
            {
                DrawGrid(_bakeSystem.World.worldGrid);
            }

            if (_config.DrawCollider)
            {
                DrawColliders(_bakeSystem.World);
            }
        }

        private void DrawColliders(ColliderWorld world)
        {
            for (var i = 0; i < world.colliderBounds.Length; i++)
            {
                var bounds = world.colliderBounds[i];
                var shape = world.colliderShapes[i];
                var center = new float2(bounds.xMin + bounds.xMax, bounds.yMin + bounds.yMax) / 2f;
                var angle = shape.rotation * FloatMath.TwoPI;
                
                _colliderGizmos.DrawWireRect(center, shape.size, angle);
            }
        }
        
        private void DrawGrid(ColliderWorldGrid worldGrid)
        {
            var cellSize = 1 << worldGrid.power;
            var cellCountX = worldGrid.size.x;
            var cellCountY = worldGrid.size.y;
            var worldMin = worldGrid.anchor * cellSize;
            var worldMax = (worldGrid.anchor + worldGrid.size) * cellSize;

            _gridGizmos.DrawLine(worldMin.x, worldMin.y, worldMin.x, worldMax.y);
            _gridGizmos.DrawLine(worldMin.x, worldMax.y, worldMax.x, worldMax.y);
            _gridGizmos.DrawLine(worldMax.x, worldMax.y, worldMax.x, worldMin.y);
            _gridGizmos.DrawLine(worldMax.x, worldMin.y, worldMin.x, worldMin.y);

            for (var i = 1; i < cellCountX; i++)
            {
                var p0 = new float2(worldMin.x + cellSize * i, worldMax.y);
                var p1 = new float2(worldMin.x + cellSize * i, worldMin.y);
                _gridGizmos.DrawLine(p0, p1);
            }
            
            for (var i = 1; i < cellCountY; i++)
            {
                var p2 = new float2(worldMin.x, worldMin.y + i * cellSize);
                var p3 = new float2(worldMax.x, worldMin.y + i * cellSize);
                _gridGizmos.DrawLine(p2, p3);
            }
        }

        public void OnFinalize()
        {
            
        }
    }
}