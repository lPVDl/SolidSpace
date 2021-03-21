using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Physics.Collision
{
    public struct ColliderComponent : IComponentData
    {
        public float radius;
    }
}