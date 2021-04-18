using Unity.Entities;

namespace SpaceSimulator.Entities.Despawn
{
    public struct DespawnComponent : IComponentData
    {
        public float despawnTime;
    }
}