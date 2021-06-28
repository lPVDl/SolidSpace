using System;
using SolidSpace.Entities.Physics.Velcast;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IKovacBakery<T> where T : struct, IColliderBakeBehaviour
    {
        BakedCollidersData Bake(ref T behaviour);
    }
}