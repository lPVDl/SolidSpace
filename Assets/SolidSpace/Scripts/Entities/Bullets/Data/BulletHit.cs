using SolidSpace.Mathematics;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Bullets
{
    internal struct BulletHit
    {
        public Entity bulletEntity;
        public int2 hitPixel;
        public AtlasIndex16 colliderSprite;
        public AtlasIndex16 colliderHealth;
        public half2 colliderSize;
        public Entity colliderEntity;
    }
}