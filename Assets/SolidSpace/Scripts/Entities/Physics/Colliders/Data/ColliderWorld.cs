using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct ColliderWorld
    {
        public NativeSlice<FloatBounds> colliders;
        public NativeSlice<ushort> colliderStream;
        public NativeSlice<ColliderListPointer> worldCells;
        public ColliderWorldGrid worldGrid;
    }
}