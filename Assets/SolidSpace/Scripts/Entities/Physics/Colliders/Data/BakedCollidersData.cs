using System;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Physics.Velcast
{
    public struct BakedCollidersData : IDisposable
    {
        public ColliderWorldGrid grid;
        public NativeArray<ColliderShape> shapes;
        public NativeArray<FloatBounds> bounds;
        public NativeArray<ColliderListPointer> cells;
        public NativeArray<ushort> indices;

        public void Dispose()
        {
            shapes.Dispose();
            bounds.Dispose();
            cells.Dispose();
            indices.Dispose();
        }
    }
}