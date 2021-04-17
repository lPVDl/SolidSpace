using SpaceSimulator.Runtime.Entities.Particles.Emission;
using Unity.Collections;

namespace SpaceSimulator.Runtime.Entities
{
    public interface IEntitySystem
    {
        public ESystemType SystemType { get; }

        public void Initialize();

        public void Update();

        public void FinalizeSystem();
    }
}