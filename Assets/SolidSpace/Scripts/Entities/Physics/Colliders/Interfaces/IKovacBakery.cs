using System;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IKovacBakery<T> where T : struct, IColliderWorld
    {
        KovacWorld<T> Bake();
    }
}