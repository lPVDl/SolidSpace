using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Bullets
{
    internal interface IBulletComputeSystem
    {
        public NativeArray<Entity> EntitiesToDestroy { get; }
    }
}