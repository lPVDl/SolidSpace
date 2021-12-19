using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Prefabs
{
    public struct EntityReplicationData
    {
        public Entity sourceEntity;
        public AtlasIndex replicationHealth;
        public byte2 replicationSize;
    }
}