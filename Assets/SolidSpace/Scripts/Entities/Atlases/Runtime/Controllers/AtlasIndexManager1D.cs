using System;
using System.Collections.Generic;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasIndexManager1D : IDisposable
    {
        private const int ChunkPerAllocation = 16;

        public NativeSlice<AtlasChunk1D> Chunks { get; private set; }
        
        private readonly AtlasSectorManager1D _sectorManager;
        private readonly int _minEntityPower;
        private readonly int _maxEntityPower;
        private readonly Stack<AtlasIndex>[] _freeIndices;
        private readonly int[] _chunkIndexPowers;
        
        private NativeArray<AtlasChunk1D> _chunks;
        private int _chunkCount;

        public AtlasIndexManager1D(AtlasSectorManager1D sectorManager, IReadOnlyList<AtlasChunk1DConfig> chunkConfig)
        {
            _sectorManager = sectorManager;
            _minEntityPower = (int) CeilLog2(chunkConfig[0].itemSize);
            _maxEntityPower = (int) CeilLog2(chunkConfig[chunkConfig.Count - 1].itemSize);
            _chunkIndexPowers = new int[_minEntityPower + 1];
            _freeIndices = new Stack<AtlasIndex>[_maxEntityPower + 1];
            for (int i = _minEntityPower, j = 0; i <= _maxEntityPower; i++, j++)
            {
                _chunkIndexPowers[i] = (byte) CeilLog2(chunkConfig[j].itemCount);
                _freeIndices[i] = new Stack<AtlasIndex>();
            }

            _chunks = NativeMemory.CreatePersistentArray<AtlasChunk1D>(0);
            Chunks = new NativeSlice<AtlasChunk1D>(_chunks, 0, 0);
        }
        
        public AtlasIndex Allocate(int size)
        {
            var entityPower = (byte) Math.Max(_minEntityPower, CeilLog2(size));
            if (entityPower > _maxEntityPower)
            {
                var message = $"Size {1 << entityPower} is more than allowed {1 << _maxEntityPower}";
                throw new InvalidOperationException(message);
            }

            var indexStack = _freeIndices[entityPower];
            if (indexStack.Count == 0)
            {
                CreateChunk(entityPower);
            }
            
            return indexStack.Pop();
        }

        public void Release(AtlasIndex atlasIndex)
        {
            var chunk = _chunks[atlasIndex.chunkId];
            chunk.GetPower(out _, out var itemPower);
            _freeIndices[itemPower].Push(atlasIndex);
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
            var chunk = new AtlasChunk1D
            {
                offset = sector
            };
            chunk.SetPower(chunkIndexPower, itemPower);
            _chunks[chunkId] = chunk;

            var indexStack = _freeIndices[itemPower];
            // TODO [T-19]: Chunk occupation, use int to store values instead on stack.
            var lastItemId = (1 << chunkIndexPower) - 1;
            for (var i = lastItemId; i >= 0; i--)
            {
                indexStack.Push(new AtlasIndex
                {
                    chunkId = (ushort) chunkId,
                    itemId = (byte) i
                });
            }

            Chunks = new NativeSlice<AtlasChunk1D>(_chunks, 0, _chunkCount);
        }
        
        private static double CeilLog2(double a)
        {
            return Math.Ceiling(Math.Log(a, 2));
        }

        public void Dispose()
        {
            _chunks.Dispose();
        }
    }
}