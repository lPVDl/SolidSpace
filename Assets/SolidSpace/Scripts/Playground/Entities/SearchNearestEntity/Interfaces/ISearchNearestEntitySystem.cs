using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Entities.SearchNearestEntity
{
    internal interface ISearchNearestEntitySystem
    {
        EntityPosition Result { get; }

        void SetSearchPosition(float2 position);
        
        void SetEnabled(bool enabled);

        void SetQuery(EntityQueryDesc queryDesc);
    }
}