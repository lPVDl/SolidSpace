using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal interface IEntityByPositionSearchSystem
    {
        EntityPosition Result { get; }

        void SetSearchPosition(float2 position);
        
        void SetEnabled(bool enabled);

        void SetQuery(EntityQueryDesc queryDesc);
    }
}