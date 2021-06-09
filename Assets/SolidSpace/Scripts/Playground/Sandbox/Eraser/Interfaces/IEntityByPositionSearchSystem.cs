using Unity.Mathematics;

namespace SolidSpace.Playground.Sandbox.Eraser
{
    internal interface IEntityByPositionSearchSystem
    {
        EntityPosition Result { get; }

        void SetSearchPosition(float2 position);
        
        void SetEnabled(bool enabled);
    }
}