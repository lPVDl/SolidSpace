using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Physics.Velcast
{
    public struct KovacRayHit
    {
        public FloatRay ray;
        public int rayIndex;
        public int writeOffset;
        public ushort colliderIndex;
    }
}