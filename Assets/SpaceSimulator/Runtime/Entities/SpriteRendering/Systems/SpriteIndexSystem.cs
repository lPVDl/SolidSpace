using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteIndexSystem : SystemBase
    {
        public NativeList<SpriteChunk> Chunks => _chunks;
        
        private const int MaxSpritePower = 6;
        private const int MinSpritePower = 2;

        private SpriteSquareSystem _squareSystem;
        private Stack<SpriteIndex>[] _emptySprites;
        private NativeList<SpriteChunk> _chunks;
        
        public SpriteIndex AllocateSpace(int2 size)
        {
            var maxSize = math.max(size.x, size.y);
            var power = (byte) math.max(math.ceil(math.log2(maxSize)), MinSpritePower);
            var indexStack = _emptySprites[power];
            if (indexStack.Count == 0)
            {
                CreateChunk(power);
            }

            return indexStack.Pop();
        }

        public void ReleaseSpace(SpriteIndex spriteIndex)
        {
            var chunk = _chunks[spriteIndex.chunkId];
            _emptySprites[chunk.power].Push(spriteIndex);
        }
        
        protected override void OnCreate()
        {
            _squareSystem = World.GetOrCreateSystem<SpriteSquareSystem>();
            _emptySprites = new Stack<SpriteIndex>[MaxSpritePower + 1];
            for (var i = 0; i <= MaxSpritePower; i++)
            {
                _emptySprites[i] = new Stack<SpriteIndex>();
            }

            _chunks = new NativeList<SpriteChunk>(16, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            
        }

        private void CreateChunk(byte power)
        {
            var chunkId = (byte) _chunks.Length;
            var square = _squareSystem.AllocateSquare((byte)(power + 4));
            var chunk = new SpriteChunk
            {
                atlasId = square.atlasId,
                offsetX = square.offsetX,
                offsetY = square.offsetY,
                power = power,
            };
            _chunks.Add(chunk);

            var indexStack = _emptySprites[power];
            for (var i = 255; i >= 0; i--)
            {
                var spriteIndex = new SpriteIndex
                {
                    itemId = (byte) i,
                    chunkId = chunkId
                };
                indexStack.Push(spriteIndex);
            }
        }

        protected override void OnDestroy()
        {
            _chunks.Dispose();
        }
    }
}