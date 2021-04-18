using Unity.Collections;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public interface ISpriteAtlasSystem
    {
        public Texture2D Texture { get; }
        
        public NativeList<SpriteAtlasChunk> Chunks { get; }

        public SpriteAtlasIndex AllocateSpace(int sizeX, int sizeY);

        public void ScheduleTextureCopy(Texture2D source, SpriteAtlasIndex target);

        public void ReleaseSpace(SpriteAtlasIndex spriteAtlasIndex);
    }
}