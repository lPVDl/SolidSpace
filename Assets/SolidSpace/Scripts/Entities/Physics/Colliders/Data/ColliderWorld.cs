using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct ColliderWorld
    {
        public NativeSlice<FloatBounds> colliderBounds;
        public NativeSlice<ColliderShape> colliderShapes;
        public NativeSlice<ushort> colliderStream;
        public NativeSlice<ColliderListPointer> worldCells;
        public ColliderWorldGrid worldGrid;
    }
}