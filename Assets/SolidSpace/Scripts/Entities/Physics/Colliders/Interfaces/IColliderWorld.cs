using System;
using SolidSpace.Entities.World;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Colliders
{
    public interface IColliderWorld : IDisposable
    {
        void Initialize(IEntityManager entityManager, int colliderCount);

        void ReadChunk(ArchetypeChunk chunk);

        void ReadEntity(int entityIndex, int writeOffset);
    }
}