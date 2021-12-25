using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public interface IHealthAtlasSystem
    {
        public NativeArray<byte> Data { get; }
        
        public NativeSlice<AtlasChunk1D> Chunks { get; }
        
        public NativeSlice<ushort> ChunksOccupation { get; }

        public AtlasIndex16 Allocate(int width, int height);
        
        public void Copy(Texture2D source, AtlasIndex16 target);

        public void Release(AtlasIndex16 index);
    }
}