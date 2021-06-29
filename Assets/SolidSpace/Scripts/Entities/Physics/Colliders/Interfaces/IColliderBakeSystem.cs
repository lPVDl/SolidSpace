using System;
using SolidSpace.Entities.Physics.Raycast;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IColliderBakeSystem<T> where T : struct, IColliderBakeBehaviour
    {
        BakedColliders Bake(ref T behaviour);
    }
}