using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    public struct DespawnComponent : IComponentData
    {
        public float despawnTime;
    }
}