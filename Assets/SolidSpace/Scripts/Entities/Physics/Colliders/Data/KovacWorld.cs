using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct KovacWorld<T> where T : struct, IColliderWorld
    {
        public T colliderData;
        public NativeArray<ushort> colliderIndices;
        public NativeArray<FloatBounds> colliderBounds;
        public NativeArray<ColliderShape> colliderShapes;
        public NativeArray<ColliderListPointer> cells;
        public ushort colliderCount;
        public ColliderWorldGrid grid;

        public void Dispose()
        {
            colliderIndices.Dispose();
            cells.Dispose();
            colliderData.Dispose();
            colliderBounds.Dispose();
            colliderShapes.Dispose();
        }
    }
}