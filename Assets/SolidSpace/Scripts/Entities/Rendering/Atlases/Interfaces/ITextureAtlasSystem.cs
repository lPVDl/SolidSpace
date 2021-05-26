using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Atlases
{
    public interface ITextureAtlasSystem
    {
        public Texture2D Texture { get; }
        
        public NativeSlice<AtlasChunk2D> Chunks { get; }

        public AtlasIndex Allocate(int width, int height);

        public void ScheduleCopy(Texture2D source, AtlasIndex target);

        public void Release(AtlasIndex atlasIndex);
    }
}