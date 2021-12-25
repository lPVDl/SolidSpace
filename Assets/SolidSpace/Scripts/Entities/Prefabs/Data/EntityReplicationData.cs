using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Prefabs
{
    public struct EntityReplicationData
    {
        public Entity sourceEntity;
        public AtlasIndex16 replicationHealth;
        public byte2 replicationSize;
    }
}