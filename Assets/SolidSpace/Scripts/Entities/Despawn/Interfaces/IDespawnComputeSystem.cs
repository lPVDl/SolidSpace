using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    public interface IDespawnComputeSystem
    {
        public NativeArray<Entity> ResultBuffer { get; }
        
        public int ResultCount { get; }
    }
}