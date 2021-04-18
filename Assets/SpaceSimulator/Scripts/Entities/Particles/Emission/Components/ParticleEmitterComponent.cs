using Unity.Entities;

namespace SpaceSimulator.Entities.Particles.Emission
{
    public struct ParticleEmitterComponent : IComponentData
    {
        public float particleVelocity;
    }
}