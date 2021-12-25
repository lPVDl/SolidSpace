using System.Collections.Generic;
using SolidSpace.Entities.Splitting;
using SolidSpace.Mathematics;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Prefabs
{
    public interface IPrefabSystem
    {
        IReadOnlyList<ComponentType> ShipComponents { get; }
        int2 ShipSize { get; }

        void SpawnShip(float2 position, float rotation);
        
        void ScheduleReplication(Entity parent, AtlasIndex16 childHealth, ByteBounds childBounds);
    }
}