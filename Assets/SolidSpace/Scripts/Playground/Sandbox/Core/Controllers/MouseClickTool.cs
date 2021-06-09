using SolidSpace.Playground.UI;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal class MouseClickTool
    {
        private readonly IUIManager _uiManager;
        private readonly Camera _camera;

        public MouseClickTool(IUIManager uiManager, Camera camera)
        {
            _uiManager = uiManager;
            _camera = camera;
        }

        public bool CheckMouseClick(out float2 position)
        {
            position = float2.zero;

            if (!Input.GetMouseButtonDown(0))
            {
                return false;
            }
            
            if (_uiManager.IsMouseOver)
            {
                return false;
            }

            return GetClickPosition(out position);
        }
        
        private bool GetClickPosition(out float2 clickPosition)
        {
            clickPosition = float2.zero;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(-Vector3.forward, Vector3.zero);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }
            
            var hitPos = ray.origin + ray.direction * distance;
            clickPosition = new float2
            {
                x = hitPos.x,
                y = hitPos.y
            };
            
            return true;
        }
    }
}