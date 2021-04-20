using Unity.Entities;

namespace SpaceSimulator.Entities.ParticleEmitters
{
    public struct ParticleEmitterComponent : IComponentData
    {
        public float particleVelocity;
    }
}