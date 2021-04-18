using Unity.Collections;
using UnityEngine;

namespace SpaceSimulator.Entities.SpriteRendering
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