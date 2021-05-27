using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public struct RaycastHit
    {
        public ushort colliderIndex;
        public Entity raycasterEntity;
        public byte raycasterArchetype;
    }
}