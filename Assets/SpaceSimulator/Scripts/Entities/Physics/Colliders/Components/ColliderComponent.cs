using Unity.Entities;

namespace SpaceSimulator.Entities.Physics
{
    public struct ColliderComponent : IComponentData
    {
        public float radius;
    }
}