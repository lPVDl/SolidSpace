using System;
using System.Collections.Generic;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasIndexManager1D : IDisposable
    {
        private const int BufferSize = 16;

        public NativeSlice<AtlasChunk1D> Chunks { get; private set; }
        public NativeSlice<ushort> ChunksOccupation { get; private set; }
        
        private readonly AtlasSectorManager1D _sectorManager;
        private readonly int _minEntityPower;
        private readonly int _maxEntityPower;
        private readonly List<ushort>[] _partiallyFilledChunks;
        private readonly Stack<ushort> _freeChunkIndices;

        private NativeArray<AtlasChunk1D> _chunks;
        private NativeArray<ushort> _chunksOccupation;

        public AtlasIndexManager1D(Atlas1DConfig config)
        {
            _sectorManager = new AtlasSectorManager1D(config.AtlasSize);
            _minEntityPower = (int) CeilLog2(config.MinItemSize);
            _maxEntityPower = (int) CeilLog2(config.MaxItemSize);
            _freeChunkIndices = new Stack<ushort>();
            _partiallyFilledChunks = new List<ushort>[_maxEntityPower + 1];
            for (var i = _minEntityPower; i < _maxEntityPower; i++)
            {
                _partiallyFilledChunks[i] = new List<ushort>();
            }

            _chunks = NativeMemory.CreatePersistentArray<AtlasChunk1D>(0);
            _chunksOccupation = NativeMemory.CreatePersistentArray<ushort>(0);
            
            Chunks = new NativeSlice<AtlasChunk1D>(_chunks, 0, 0);
            ChunksOccupation = new NativeSlice<ushort>(_chunksOccupation, 0, 0);
        }
        
        public AtlasIndex Allocate(int size)
        {
            var entityPower = (byte) Math.Max(_minEntityPower, CeilLog2(size));
            if (entityPower > _maxEntityPower)
            {
                var message = $"Size {1 << entityPower} is more than allowed {1 << _maxEntityPower}";
                throw new InvalidOperationException(message);
            }

            var chunkStack = _partiallyFilledChunks[entityPower];
            if (chunkStack.Count == 0)
            {
                CreateChunk(entityPower);
            }

            var chunkId = chunkStack[0];
            var freeIndexMask = ~_chunksOccupation[chunkId];
            var itemIndex = BinaryMath.GetFirstBitIndex16((ushort) freeIndexMask);
            freeIndexMask &= ~(1 << itemIndex);
            
            _chunksOccupation[chunkId] = (ushort) ~freeIndexMask;

            if ((ushort) freeIndexMask == 0)
            {
                chunkStack.RemoveAt(0);
            }

            return new AtlasIndex
            {
                itemId = (byte) itemIndex,
                chunkId = chunkId
            };
        }

        public void Release(AtlasIndex index)
        {
            var chunk = _chunks[index.chunkId];
            var chunkOccupation = _chunksOccupation[index.chunkId];
            var itemPower = chunk.itemPower;

            if ((chunkOccupation & (1 << index.itemId)) == 0)
            {
                throw new InvalidOperationException($"Can not release '{index}'. It was not allocated yet");
            }
            
            if (chunkOccupation == ushort.MaxValue)
            {
                _partiallyFilledChunks[itemPower].Insert(0, index.chunkId);
            }

            chunkOccupation = (ushort) (chunkOccupation & ~(1 << index.itemId));
            _chunksOccupation[index.chunkId] = chunkOccupation;

            if (chunkOccupation == 0)
            {
                _partiallyFilledChunks[itemPower].Remove(index.chunkId);
                _sectorManager.Release(chunk.offset, itemPower + 4);
                _freeChunkIndices.Push(index.chunkId);
            }
        }

        private void CreateChunk(int itemPower)
        {
            if (_freeChunkIndices.Count == 0)
            {
                CreateChunkIndices(BufferSize);
            }
            
            var chunkId = _freeChunkIndices.Pop();
            var sector = _sectorManager.Allocate(itemPower + 4);
            var chunk = new AtlasChunk1D
            {
                offset = sector
            };
            chunk.itemPower = (byte) itemPower;
            
            _chunks[chunkId] = chunk;
            _chunksOccupation[chunkId] = 0;
            _partiallyFilledChunks[itemPower].Add(chunkId);
        }

        private void CreateChunkIndices(int newIndexCount)
        {
            var rule = new ArrayMaintenanceData
            {
                requiredCapacity = _chunks.Length + newIndexCount,
                itemPerAllocation = newIndexCount,
                copyOnResize = true
            };
            NativeMemory.MaintainPersistentArrayLength(ref _chunks, rule);
            NativeMemory.MaintainPersistentArrayLength(ref _chunksOccupation, rule);
            Chunks = new NativeSlice<AtlasChunk1D>(_chunks, 0, _chunks.Length);
            ChunksOccupation = new NativeSlice<ushort>(_chunksOccupation, 0, _chunksOccupation.Length);

            for (var i = _chunks.Length - newIndexCount; i < _chunks.Length; i++)
            {
                _chunks[i] = default;
                _chunksOccupation[i] = 0;
                _freeChunkIndices.Push((ushort) i);
            }
        }

        private static double CeilLog2(double a)
        {
            return Math.Ceiling(Math.Log(a, 2));
        }

        public void Dispose()
        {
            _chunks.Dispose();
            _chunksOccupation.Dispose();
        }
    }
}