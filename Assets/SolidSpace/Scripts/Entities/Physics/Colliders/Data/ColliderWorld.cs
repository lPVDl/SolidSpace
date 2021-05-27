using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct ColliderWorld
    {
        public NativeSlice<EntityArchetype> archetypes;
        public NativeSlice<byte> colliderArchetypes;
        public NativeSlice<FloatBounds> colliderBounds;
        public NativeSlice<ColliderShape> colliderShapes;
        public NativeSlice<ushort> colliderStream;
        public NativeSlice<ColliderListPointer> worldCells;
        public ColliderWorldGrid worldGrid;
    }
}