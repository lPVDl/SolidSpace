using Unity.Entities;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles
{
    public struct TriangleParticleEmitterComponent : IComponentData
    {
        public float spawnDelay;
        public float timer;
        public float seed;
    }
}