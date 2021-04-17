using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public interface IRaycastComputeSystem
    {
        NativeArray<Entity> HitEntities { get; }
        int HitCount { get; }
    }
}