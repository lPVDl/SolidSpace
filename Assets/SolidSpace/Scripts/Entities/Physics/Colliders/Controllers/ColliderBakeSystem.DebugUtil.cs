using SolidSpace.DebugUtils;
using UnityEngine;

namespace SolidSpace.Entities.Physics
{
    public partial class ColliderBakeSystem
    {
        private struct DebugUtil
        {
            public void LogWorld(ColliderWorldGrid worldGrid)
            {
                var cellSize = 1 << worldGrid.power;
                var cellCountX = worldGrid.size.x;
                var cellCountY = worldGrid.size.y;
                var worldMin = worldGrid.anchor * cellSize;
                var worldMax = (worldGrid.anchor + worldGrid.size) * cellSize;
            
                SpaceDebug.LogState("ColliderCellCountX", cellCountX);
                SpaceDebug.LogState("ColliderCellCountY", cellCountY);
                SpaceDebug.LogState("ColliderCellSize", cellSize);
            
                Debug.DrawLine(new Vector3(worldMin.x, worldMin.y), new Vector3(worldMin.x, worldMax.y));
                Debug.DrawLine(new Vector3(worldMin.x, worldMax.y), new Vector3(worldMax.x, worldMax.y));
                Debug.DrawLine(new Vector3(worldMax.x, worldMax.y), new Vector3(worldMax.x, worldMin.y));
                Debug.DrawLine(new Vector3(worldMax.x, worldMin.y), new Vector3(worldMin.x, worldMin.y));

                for (var i = 1; i < cellCountX; i++)
                {
                    var p0 = new Vector3(worldMin.x + cellSize * i, worldMax.y, 0);
                    var p1 = new Vector3(worldMin.x + cellSize * i, worldMin.y, 0);
                    Debug.DrawLine(p0, p1);
                }
            
                for (var i = 1; i < cellCountY; i++)
                {
                    var p2 = new Vector3(worldMin.x, worldMin.y + i * cellSize, 0);
                    var p3 = new Vector3(worldMax.x, worldMin.y + i * cellSize, 0);
                    Debug.DrawLine(p2, p3);
                }
            }
        }
    }
}