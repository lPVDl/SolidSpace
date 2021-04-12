using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public struct DespawnComponent : IComponentData
    {
        public float despawnTime;
    }
}