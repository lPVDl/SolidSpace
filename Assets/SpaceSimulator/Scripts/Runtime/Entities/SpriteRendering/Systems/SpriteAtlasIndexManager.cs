using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasIndexManager : IDisposable
    {
        private const int MinSpritePower = 2;

        public NativeList<SpriteAtlasChunk> Chunks => _chunks;

        private NativeList<SpriteAtlasChunk> _chunks;
        private readonly SpriteAtlasSquareManager _squareManager;
        private readonly Stack<SpriteAtlasIndex>[] _emptySprites;
        private readonly int _maxSpritePower;
        private readonly int[] _chunkIndexPowers;

        public SpriteAtlasIndexManager(SpriteAtlasSquareManager squareManager, int[] chunkIndexPowers)
        {
            if (chunkIndexPowers == null || chunkIndexPowers.Length < MinSpritePower + 1)
            {
                var message = $"{nameof(chunkIndexPowers)} must contain at least {MinSpritePower} elements";
                throw new ArgumentException(message);
            }
            
            _maxSpritePower = chunkIndexPowers.Length - 1;
            _squareManager = squareManager;
            _chunkIndexPowers = chunkIndexPowers;
            _emptySprites = new Stack<SpriteAtlasIndex>[_maxSpritePower + 1];
            for (var i = 0; i <= _maxSpritePower; i++)
            {
                _emptySprites[i] = new Stack<SpriteAtlasIndex>();
            }

            _chunks = new NativeList<SpriteAtlasChunk>(16, Allocator.Persistent);
        }

        public SpriteAtlasIndex AllocateSpace(int sizeX, int sizeY)
        {
            var maxSize = math.max(sizeX, sizeY);
            var spritePower = (byte) math.max(math.ceil(math.log2(maxSize)), MinSpritePower);
            if (spritePower > _maxSpritePower)
            {
                var message = $"Size {1 << spritePower} is more than allowed {1 << _maxSpritePower}";
                throw new InvalidOperationException(message);
            }

            var indexStack = _emptySprites[spritePower];
            if (indexStack.Count == 0)
            {
                CreateChunk(spritePower);
            }

            return indexStack.Pop();
        }

        public void ReleaseSpace(SpriteAtlasIndex spriteAtlasIndex)
        {
            var chunk = _chunks[spriteAtlasIndex.chunkId];
            chunk.GetPower(out _, out var itemPower);
            _emptySprites[itemPower].Push(spriteAtlasIndex);
        }

        private void CreateChunk(int spritePower)
        {
            var chunkId = _chunks.Length;
            var chunkIndexPower = _chunkIndexPowers[spritePower];
            var square = _squareManager.AllocateSquare(spritePower + chunkIndexPower);
            var chunk = new SpriteAtlasChunk
            {
                offsetX = square.offsetX,
                offsetY = square.offsetY,
            };
            chunk.SetPower(chunkIndexPower, spritePower);
            _chunks.Add(chunk);

            var indexStack = _emptySprites[spritePower];
            var lastItemId = (1 << (chunkIndexPower << 1)) - 1;
            for (var i = lastItemId; i >= 0; i--)
            {
                var spriteIndex = new SpriteAtlasIndex
                {
                    chunkId = (ushort) chunkId,
                    itemId = (byte) i
                };
                indexStack.Push(spriteIndex);
            }
        }

        public void Dispose()
        {
            _chunks.Dispose();
        }
    }
}