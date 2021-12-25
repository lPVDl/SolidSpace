using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public interface ISpriteColorSystem
    {
        public Texture2D Texture { get; }
        
        public NativeSlice<AtlasChunk2D> Chunks { get; }
        
        public NativeSlice<ushort> ChunksOccupation { get; }

        public AtlasIndex16 Allocate(int width, int height);
        
        public void Release(AtlasIndex16 index);
        
        public void Copy(Texture2D source, AtlasIndex16 target);
    }
}