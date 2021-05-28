using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public struct RaycastWorld
    {
        public NativeSlice<EntityArchetype> raycastArchetypes;

        public NativeSlice<Entity> raycastEntities;
        public NativeSlice<ushort> colliderIndices;
        public NativeSlice<byte> raycastArchetypeIndices;
    }
}