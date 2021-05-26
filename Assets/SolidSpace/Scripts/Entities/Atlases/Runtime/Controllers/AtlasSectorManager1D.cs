using System;
using System.Collections.Generic;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasSectorManager1D
    {
        private readonly Stack<ushort>[] _emptySectors;
        private readonly byte _atlasPower;
        
        public AtlasSectorManager1D(int atlasSize)
        {
            _atlasPower = (byte) Math.Ceiling(Math.Log(atlasSize, 2));
            _emptySectors = new Stack<ushort>[_atlasPower + 1];
            
            for (var i = 0; i < _atlasPower; i++)
            {
                _emptySectors[i] = new Stack<ushort>();
            }
            
            _emptySectors[_atlasPower].Push(0);
        }

        public ushort Allocate(int power)
        {
            var sectorStack = _emptySectors[power];
            if (sectorStack.Count > 0)
            {
                return sectorStack.Pop();
            }

            for (var i = power + 1; i <= _atlasPower; i++)
            {
                sectorStack = _emptySectors[i];
                if (sectorStack.Count == 0)
                {
                    continue;
                }

                var sector = sectorStack.Pop();
                for (var j = i - 1; j >= power; j--)
                {
                    var size = 1 << (j - 1);
                    sectorStack = _emptySectors[j];
                    sectorStack.Push((ushort) (sector + size));
                }

                return sector;
            }

            throw new OutOfMemoryException($"Failed to allocate sector with size {1 << power}");
        }
    }
}