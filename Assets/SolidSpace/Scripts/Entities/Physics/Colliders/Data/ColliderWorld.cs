using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct ColliderWorld
    {
        public NativeSlice<EntityArchetype> archetypes;
        public NativeSlice<byte> colliderArchetypeIndices;
        public NativeSlice<FloatBounds> colliderBounds;
        public NativeSlice<ColliderShape> colliderShapes;
        public NativeSlice<Entity> colliderEntities;
        public NativeSlice<ushort> colliderStream;
        public NativeSlice<ColliderListPointer> worldCells;
        public ColliderWorldGrid worldGrid;
    }
}