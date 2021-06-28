using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Velcast
{
    public interface IRaycastBehaviour
    {
        void Initialize(int maxHitCount);
        void ReadChunk(ArchetypeChunk chunk);
        FloatRay GetRay(int rayIndex);
        bool TryRegisterHit(KovacRayHit hit);
        void CollectResult(NativeArray<int> offsets, NativeArray<int> counts);
    }
}