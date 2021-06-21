using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Entities.SearchNearestEntity
{
    public struct EntityPosition
    {
        public Entity entity;
        public float2 position;
        public bool isValid;
    }
}