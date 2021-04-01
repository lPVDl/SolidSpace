using Unity.Collections;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public struct ColliderWorld
    {
        public NativeSlice<ColliderBounds> colliders;
        public NativeSlice<ushort> colliderStream;
        public NativeSlice<ColliderListPointer> worldCells;
        public ColliderWorldGrid worldGrid;
    }
}