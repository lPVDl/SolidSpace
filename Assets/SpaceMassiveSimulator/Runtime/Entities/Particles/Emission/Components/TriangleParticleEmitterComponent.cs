using Unity.Entities;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles.Emission
{
    public struct TriangleParticleEmitterComponent : IComponentData
    {
        public float spawnDelay;
        public float timer;
        public float seed;
    }
}