using SolidSpace.Entities.World;
using SolidSpace.Mathematics;
using Unity.Collections;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Atlases
{
    public interface IAtlasSystem
    {
        public Texture2D Texture { get; }
        
        public NativeList<AtlasChunk> Chunks { get; }

        public AtlasIndex AllocateSpace(int sizeX, int sizeY);

        public void ScheduleTextureCopy(Texture2D source, AtlasIndex target);

        public void ReleaseSpace(AtlasIndex atlasIndex);
    }
}