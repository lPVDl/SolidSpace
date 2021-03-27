using Unity.Collections;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public struct ColliderWorld
    {
        public NativeSlice<ColliderBounds> colliders;
    }
}