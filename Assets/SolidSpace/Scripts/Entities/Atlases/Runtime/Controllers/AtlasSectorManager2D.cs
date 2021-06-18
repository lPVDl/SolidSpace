using System;
using System.Collections.Generic;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasSectorManager2D
    {
        private readonly Stack<byte2>[] _emptySectors;
        private readonly byte _atlasPower;
        
        public AtlasSectorManager2D(int atlasSize)
        {
            _atlasPower = (byte) Math.Ceiling(Math.Log(atlasSize, 2));
            _emptySectors = new Stack<byte2>[_atlasPower + 1];
            
            for (var i = 0; i <= _atlasPower; i++)
            {
                _emptySectors[i] = new Stack<byte2>();
            }
            
            _emptySectors[_atlasPower].Push(byte2.zero);
        }

        public byte2 Allocate(int power)
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
                    var size = 1 << (j - 2);
                    sectorStack = _emptySectors[j];
                    sectorStack.Push(new byte2
                    {
                        x = (byte)(sector.x + size),
                        y = sector.y
                    });
                    sectorStack.Push(new byte2
                    {
                        x = sector.x,
                        y = (byte)(sector.y + size)
                    });
                    sectorStack.Push(new byte2
                    {
                        x = (byte)(sector.x + size),
                        y = (byte)(sector.y + size)
                    });
                }

                return sector;
            }

            throw new OutOfMemoryException($"Failed to allocate sector {1 << power}x{1 << power}");
        }

        public void Release(byte2 offset, int power)
        {
            _emptySectors[power].Push(offset);
        }
    }
}