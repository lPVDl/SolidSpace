using SolidSpace.Entities.Physics.Colliders;

namespace SolidSpace.Entities.Physics.Raycast
{
    public interface IRaycastSystem<T> where T : struct, IRaycastBehaviour
    {
        void Raycast(BakedColliders colliders, ref T behaviour);
    }
}