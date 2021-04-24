using Unity.Collections;

namespace SolidSpace.Entities.ParticleEmitters
{
    public interface IParticleEmitterComputeSystem
    {
        NativeArray<ParticleEmitterData> Particles { get; }
        int ParticleCount { get; }
    }
}