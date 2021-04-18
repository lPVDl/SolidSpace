using Unity.Collections;

namespace SpaceSimulator.Entities.Particles.Emission
{
    public interface IParticleEmitterComputeSystem
    {
        NativeArray<ParticleEmissionData> Particles { get; }
        int ParticleCount { get; }
    }
}