using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics
{
    internal interface IRaycastComputeSystem
    {
        NativeArray<Entity> HitEntities { get; }
        int HitCount { get; }
    }
}