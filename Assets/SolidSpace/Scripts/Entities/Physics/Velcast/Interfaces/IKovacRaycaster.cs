namespace SolidSpace.Entities.Physics.Velcast
{
    public interface IKovacRaycaster<T> where T : struct, IRaycastBehaviour
    {
        void Raycast(BakedCollidersData colliders, ref T behaviour);
    }
}