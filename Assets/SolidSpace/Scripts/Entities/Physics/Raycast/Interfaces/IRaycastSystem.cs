using SolidSpace.Entities.Physics.Colliders;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public interface IRaycastSystem<T> where T : struct, IRaycastBehaviour
    {
        void Raycast(BakedColliders colliders, NativeArray<ArchetypeChunk> archetypeChunks, ref T behaviour);
    }
}