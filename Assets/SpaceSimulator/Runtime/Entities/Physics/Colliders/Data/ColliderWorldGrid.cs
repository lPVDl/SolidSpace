using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public struct ColliderWorldGrid
    {
        public int2 cellCount;
        public float2 worldMin;
        public float2 worldMax;
        public float2 cellSize;
    }
}