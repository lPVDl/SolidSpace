using System;
using System.Collections.Generic;
using Unity.Collections;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasIndexManager : IDisposable
    {
        public const int MinSpritePower = 2;

        public NativeList<SpriteAtlasChunk> Chunks => _chunks;

        private NativeList<SpriteAtlasChunk> _chunks;
        private readonly SpriteAtlasSquareManager _squareManager;
        private readonly Stack<SpriteAtlasIndex>[] _emptySprites;
        private readonly int _maxSpritePower;
        private readonly int _minSpritePower;
        private readonly int[] _chunkIndexPowers;

        public SpriteAtlasIndexManager(SpriteAtlasSquareManager squareManager, IReadOnlyList<SpriteAtlasChunkConfig> chunkConfig)
        {
            _minSpritePower = (int) CeilLog2(chunkConfig[0].spriteSize);
            _maxSpritePower = (int) CeilLog2(chunkConfig[chunkConfig.Count - 1].spriteSize);
            _chunkIndexPowers = new int[_maxSpritePower + 1];
            _emptySprites = new Stack<SpriteAtlasIndex>[_maxSpritePower + 1];
            for (int i = _minSpritePower, j = 0; i <= _maxSpritePower; i++, j++)
            {
                _chunkIndexPowers[i] = (byte) CeilLog4(chunkConfig[j].itemCount);
                _emptySprites[i] = new Stack<SpriteAtlasIndex>();
            }
            
            _chunks = new NativeList<SpriteAtlasChunk>(16, Allocator.Persistent);
            _squareManager = squareManager;
        }

        public SpriteAtlasIndex AllocateSpace(int sizeX, int sizeY)
        {
            var maxSize = Math.Max(sizeX, sizeY);
            var spritePower = (byte) Math.Max(_minSpritePower, CeilLog2(maxSize));
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

        private static double CeilLog2(double a)
        {
            return Math.Ceiling(Math.Log(a, 2));
        }

        private static double CeilLog4(double a)
        {
            return Math.Ceiling(Math.Log(a, 2));
        }

        public void Dispose()
        {
            _chunks.Dispose();
        }
    }
}