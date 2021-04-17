using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public interface IDespawnComputeSystem
    {
        public NativeArray<Entity> ResultBuffer { get; }
        
        public int ResultCount { get; }
    }
}