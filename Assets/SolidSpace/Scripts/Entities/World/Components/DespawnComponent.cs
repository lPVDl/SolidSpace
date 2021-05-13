using Unity.Entities;

namespace SolidSpace.Entities.World
{
    public struct DespawnComponent : IComponentData
    {
        public float despawnTime;
    }
}