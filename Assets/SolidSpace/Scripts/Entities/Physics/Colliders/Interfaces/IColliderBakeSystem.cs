using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IColliderBakeSystem<T> where T : struct, IColliderBakeBehaviour
    {
        BakedColliders Bake(NativeArray<ArchetypeChunk> archetypeChunks, ref T behaviour);
    }
}