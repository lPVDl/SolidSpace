using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.Core
{
    public interface IPlaygroundTool
    {
        Sprite Icon { get; }
        
        void Initialize();
        void OnMouseClick(float2 clickPosition);
        void OnToolSelected();
        void OnToolDeselected();

        void FinalizeTool();
    }
}