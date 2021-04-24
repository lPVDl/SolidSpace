using Unity.Entities;

namespace SolidSpace.Entities.ParticleEmitters
{
    public struct ParticleEmitterComponent : IComponentData
    {
        public float particleVelocity;
    }
}