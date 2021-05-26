using System;
using System.Collections.Generic;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasIndexManager2D : IDisposable
    {
        private const int ChunkPerAllocation = 16;
        
        public NativeSlice<AtlasChunk2D> Chunks { get; private set; }

        private readonly AtlasSectorManager2D _sectorManager;
        private readonly Stack<AtlasIndex>[] _freeIndices;
        private readonly int _maxEntityPower;
        private readonly int _minEntityPower;
        private readonly int[] _chunkIndexPowers;
        
        private NativeArray<AtlasChunk2D> _chunks;
        private int _chunkCount;

        public AtlasIndexManager2D(AtlasSectorManager2D sectorManager, IReadOnlyList<AtlasChunk2DConfig> chunkConfig)
        {
            _sectorManager = sectorManager;
            _minEntityPower = (int) CeilLog2(chunkConfig[0].itemSize);
            _maxEntityPower = (int) CeilLog2(chunkConfig[chunkConfig.Count - 1].itemSize);
            _chunkIndexPowers = new int[_maxEntityPower + 1];
            _freeIndices = new Stack<AtlasIndex>[_maxEntityPower + 1];
            for (int i = _minEntityPower, j = 0; i <= _maxEntityPower; i++, j++)
            {
                _chunkIndexPowers[i] = (byte) CeilLog4(chunkConfig[j].itemCount);
                _freeIndices[i] = new Stack<AtlasIndex>();
            }

            _chunks = NativeMemory.CreatePersistentArray<AtlasChunk2D>(0);
            Chunks = new NativeSlice<AtlasChunk2D>(_chunks, 0, 0);
        }

        public AtlasIndex Allocate(int width, int height)
        {
            var maxSize = Math.Max(width, height);
            var spritePower = (byte) Math.Max(_minEntityPower, CeilLog2(maxSize));
            if (spritePower > _maxEntityPower)
            {
                var message = $"Size {1 << spritePower} is more than allowed {1 << _maxEntityPower}";
                throw new InvalidOperationException(message);
            }

            var indexStack = _freeIndices[spritePower];
            if (indexStack.Count == 0)
            {
                CreateChunk(spritePower);
            }

            return indexStack.Pop();
        }

        public void Release(AtlasIndex spriteAtlasIndex)
        {
            var chunk = _chunks[spriteAtlasIndex.chunkId];
            chunk.GetPower(out _, out var itemPower);
            _freeIndices[itemPower].Push(spriteAtlasIndex);
        }

        private void CreateChunk(int itemPower)
        {
            var chunkId = _chunkCount++;
            NativeMemory.MaintainPersistentArrayLength(ref _chunks, new ArrayMaintenanceData
            {
                requiredCapacity = _chunkCount,
                itemPerAllocation = ChunkPerAllocation,
                copyOnResize = true
            });
            
            var chunkIndexPower = _chunkIndexPowers[itemPower];
            var sector = _sectorManager.Allocate(itemPower + chunkIndexPower);
            var chunk = new AtlasChunk2D
            {
                offset = new byte2(sector.x, sector.y)
            };
            chunk.SetPower(chunkIndexPower, itemPower);
            _chunks[chunkId] = chunk;

            var indexStack = _freeIndices[itemPower];
            // TODO [T-19]: Chunk occupation, use int to store values instead on stack.
            var lastItemId = (1 << (chunkIndexPower << 1)) - 1;
            for (var i = lastItemId; i >= 0; i--)
            {
                indexStack.Push(new AtlasIndex
                {
                    chunkId = (ushort) chunkId,
                    itemId = (byte) i
                });
            }
            
            Chunks = new NativeSlice<AtlasChunk2D>(_chunks, 0, _chunkCount);
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