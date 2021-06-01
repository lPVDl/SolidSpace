using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Bullets
{
    internal struct BulletHit
    {
        public Entity bulletEntity;
        public int healthOffset;
        public ushort2 spriteOffset;
    }
}