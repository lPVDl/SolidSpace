using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public interface ITextureAtlas
    {
        public Texture2D Texture { get; }
        
        public NativeSlice<AtlasChunk2D> Chunks { get; }
        
        public NativeSlice<ushort> ChunksOccupation { get; }

        public AtlasIndex Allocate(int width, int height);

        public void Release(AtlasIndex index);
    }
}