using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public struct ColliderComponent : IComponentData
    {
        public float radius;
    }
}