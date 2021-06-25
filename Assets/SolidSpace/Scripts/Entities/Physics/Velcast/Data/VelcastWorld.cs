using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Velcast
{
    public struct VelcastWorld
    {
        public NativeSlice<EntityArchetype> raycastArchetypes;

        public NativeSlice<Entity> raycastEntities;
        public NativeSlice<ushort> colliderIndices;
        public NativeSlice<FloatRay> raycastOrigins;
        public NativeSlice<byte> raycastArchetypeIndices;
    }
}