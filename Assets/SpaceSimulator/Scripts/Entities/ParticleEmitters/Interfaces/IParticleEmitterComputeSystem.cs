using Unity.Collections;

namespace SpaceSimulator.Entities.ParticleEmitters
{
    public interface IParticleEmitterComputeSystem
    {
        NativeArray<ParticleEmitterData> Particles { get; }
        int ParticleCount { get; }
    }
}