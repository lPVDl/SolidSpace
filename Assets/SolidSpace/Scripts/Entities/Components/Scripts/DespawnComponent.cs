using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct DespawnComponent : IComponentData
    {
        public float despawnTime;
    }
}