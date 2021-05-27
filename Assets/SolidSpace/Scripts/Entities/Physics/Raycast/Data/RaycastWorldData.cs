using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public struct RaycastWorldData
    {
        public NativeSlice<EntityArchetype> archetypes;
        public NativeSlice<RaycastHit> hits;
    }
}