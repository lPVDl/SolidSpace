using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public interface ISpriteFrameSystem
    {
        public Texture2D AtlasTexture { get; }
        public NativeSlice<AtlasChunk2D> Chunks { get; }
        public NativeSlice<ulong> ChunksOccupation { get; }
        int2 AtlasSize { get;  }

        public AtlasIndex64 Allocate(int width, int height);
        
        public void Release(AtlasIndex64 index);

        NativeArray<float> GetAtlasData(bool readOnly);
    }
}