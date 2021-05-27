using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Health.Atlases.Interfaces
{
    public interface ILinearAtlas<T> where T : struct
    {
        public NativeArray<T> Data { get; }
        
        public NativeSlice<AtlasChunk1D> Chunks { get; }

        public AtlasIndex Allocate(int size);

        public void Release(AtlasIndex index);
    }
}