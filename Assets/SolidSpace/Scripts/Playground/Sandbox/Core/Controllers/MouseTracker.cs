using SolidSpace.GameCycle;
using SolidSpace.Playground.Sandbox.Core;
using SolidSpace.Playground.UI;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal class MouseTracker : IPointerTracker, IController
    {
        public EControllerType ControllerType => EControllerType.UI;
        
        public bool ClickedThisFrame =>  Input.GetMouseButtonDown(0);
        
        public float2 Position { get; private set; }
        
        private readonly Camera _camera;

        public MouseTracker(Camera camera)
        {
            _camera = camera;
        }
        
        public void InitializeController()
        {
            
        }
        
        public void UpdateController()
        {
            if (GetClickPosition(out var position))
            {
                Position = position;
            }
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

        public void FinalizeController()
        {
            
        }
    }
}