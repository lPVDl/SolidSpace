using Unity.Collections;

namespace SolidSpace.Entities.Physics
{
    public struct ColliderWorld
    {
        public NativeSlice<FloatBounds> colliders;
        public NativeSlice<ushort> colliderStream;
        public NativeSlice<ColliderListPointer> worldCells;
        public ColliderWorldGrid worldGrid;
    }
}